using Core.DTOs;
using Core.DTOs.ResearchLeads;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using System.Text;

namespace Application.Services
{
    public class ResearchLeadService : IResearchLeadService
    {
        private const int MaxNotesLength = 4000;

        private readonly IResearchLeadRepository _leadRepository;
        private readonly IStaffRepository _staffRepository;

        public ResearchLeadService(
            IResearchLeadRepository leadRepository,
            IStaffRepository staffRepository)
        {
            _leadRepository = leadRepository;
            _staffRepository = staffRepository;
        }

        // What the research agent calls. Leads we already know about are skipped, not
        // errors, so the agent can safely run again and again.
        public async Task<IngestResultDto> IngestAsync(IngestResearchLeadsDto dto)
        {
            var result = new IngestResultDto();
            var prepared = dto.Leads.Select(Prepare).ToList();

            var existing = await _leadRepository.FindExistingAsync(
                prepared.Where(p => p.Host != null).Select(p => p.Host!).Distinct().ToList(),
                prepared.Select(p => p.NameKey).Distinct().ToList());

            var outreachKeys = await LoadOutreachKeysAsync();

            var seenHosts = new HashSet<string>();
            var seenKeys = new HashSet<string>();
            var toAdd = new List<ResearchLead>();

            foreach (var item in prepared)
            {
                var reason = SkipReason(item, existing, outreachKeys, seenHosts, seenKeys);
                if (reason is not null)
                {
                    result.Skipped.Add(new SkippedLeadDto
                    {
                        CompanyName = item.Input.CompanyName.Trim(),
                        Reason = reason
                    });
                    continue;
                }

                seenKeys.Add(item.NameKey);
                if (item.Host is not null)
                {
                    seenHosts.Add(item.Host);
                }

                toAdd.Add(Build(item, addedByStaffId: null, runId: Clean(dto.RunId)));
            }

            if (toAdd.Count > 0)
            {
                await _leadRepository.AddRangeAsync(toAdd);
            }

            result.Added = toAdd.Count;
            return result;
        }

        public async Task<ResearchLeadDto> AddManualAsync(ResearchLeadInputDto dto, int staffId)
        {
            var staff = await RequireStaffAsync(staffId);
            var item = Prepare(dto);

            var existing = await _leadRepository.FindExistingAsync(
                item.Host is null ? new List<string>() : new List<string> { item.Host },
                new List<string> { item.NameKey });

            var reason = SkipReason(
                item,
                existing,
                await LoadOutreachKeysAsync(),
                new HashSet<string>(),
                new HashSet<string>());

            if (reason is not null)
            {
                throw new InvalidOperationException($"We can't add that one. {reason}");
            }

            var lead = Build(item, staff.Id, runId: null);
            await _leadRepository.AddRangeAsync(new[] { lead });

            var saved = await _leadRepository.GetByIdWithDetailsAsync(lead.Id);
            return ToDto(saved!);
        }

        public async Task<PagedResultDto<ResearchLeadDto>> GetAllAsync(
            int page, int pageSize, ResearchLeadStatus? status, ResearchLeadCase? leadCase)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 15;

            var leads = await _leadRepository.GetPageAsync(page, pageSize, status, leadCase);
            var total = await _leadRepository.CountAsync(status, leadCase);

            return new PagedResultDto<ResearchLeadDto>
            {
                Data = leads.Select(ToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<int> GetNewCountAsync() => _leadRepository.CountNewAsync();

        public async Task<ResearchLeadDto?> GetByIdAsync(int id)
        {
            var lead = await _leadRepository.GetByIdWithDetailsAsync(id);
            return lead is null ? null : ToDto(lead);
        }

        // Accepting creates the outreach company and marks the lead in one save, so
        // you can never end up with one without the other.
        public async Task<ResearchLeadDto?> AcceptAsync(int id, int staffId)
        {
            var lead = await _leadRepository.GetByIdWithDetailsAsync(id);
            if (lead is null)
            {
                return null;
            }

            var staff = await RequireStaffAsync(staffId);
            RequireNew(lead);

            var now = DateTime.UtcNow;
            var actor = $"{staff.Name} {staff.Surname}".Trim();

            var company = new OutreachCompany
            {
                CompanyName = lead.CompanyName,
                Email = lead.Email,
                PhoneNumber = lead.PhoneNumber,
                Notes = BuildNotes(lead),
                Status = OutreachStatus.Researching,
                AddedByStaffId = staff.Id,
                CreatedAt = now,
                UpdatedAt = now,
                Logs = new List<OutreachLog>
                {
                    new()
                    {
                        StaffId = staff.Id,
                        ActorName = actor,
                        Message = lead.AddedByStaffId is null
                            ? "added this company from a lead suggested by the research agent."
                            : "added this company from a research lead.",
                        CreatedAt = now
                    }
                }
            };

            lead.Status = ResearchLeadStatus.Accepted;
            lead.ReviewedByStaffId = staff.Id;
            lead.ReviewedByStaff = staff;
            lead.ReviewedAt = now;
            lead.OutreachCompany = company;

            await _leadRepository.SaveChangesAsync();
            return ToDto(lead);
        }

        public async Task<ResearchLeadDto?> DismissAsync(int id, DismissResearchLeadDto dto, int staffId)
        {
            var lead = await _leadRepository.GetByIdWithDetailsAsync(id);
            if (lead is null)
            {
                return null;
            }

            var staff = await RequireStaffAsync(staffId);
            RequireNew(lead);

            lead.Status = ResearchLeadStatus.Dismissed;
            lead.DismissReason = Clean(dto.Reason);
            lead.ReviewedByStaffId = staff.Id;
            lead.ReviewedByStaff = staff;
            lead.ReviewedAt = DateTime.UtcNow;

            await _leadRepository.SaveChangesAsync();
            return ToDto(lead);
        }

        // ---------- helpers ----------

        private sealed record Prepared(ResearchLeadInputDto Input, string? Website, string? Host, string NameKey);

        private static Prepared Prepare(ResearchLeadInputDto input)
        {
            string? website = null;
            string? host = null;

            if (ResearchLeadNormalizer.TryNormalizeWebsite(input.Website, out var url, out var rawHost))
            {
                website = url;
                host = ResearchLeadNormalizer.DedupeHost(rawHost);
            }

            return new Prepared(input, website, host, ResearchLeadNormalizer.NameKey(input.CompanyName));
        }

        private static string? SkipReason(
            Prepared item,
            List<ResearchLead> existing,
            HashSet<string> outreachKeys,
            HashSet<string> seenHosts,
            HashSet<string> seenKeys)
        {
            var match = existing.FirstOrDefault(e =>
                e.NameKey == item.NameKey ||
                (item.Host is not null && e.WebsiteHost == item.Host));

            if (match is not null)
            {
                return match.Status == ResearchLeadStatus.Dismissed
                    ? "Dismissed before."
                    : "Already a lead.";
            }

            if (outreachKeys.Contains(item.NameKey))
            {
                return "Already in outreach.";
            }

            if (seenKeys.Contains(item.NameKey) || (item.Host is not null && seenHosts.Contains(item.Host)))
            {
                return "Duplicate in this run.";
            }

            return null;
        }

        private async Task<HashSet<string>> LoadOutreachKeysAsync()
        {
            var names = await _leadRepository.GetOutreachCompanyNamesAsync();
            return names
                .Select(ResearchLeadNormalizer.NameKey)
                .Where(key => key.Length > 0)
                .ToHashSet();
        }

        private static ResearchLead Build(Prepared item, int? addedByStaffId, string? runId)
        {
            var input = item.Input;

            return new ResearchLead
            {
                CompanyName = input.CompanyName.Trim(),
                Website = item.Website,
                WebsiteHost = item.Host,
                NameKey = item.NameKey,
                Industry = Clean(input.Industry),
                Location = Clean(input.Location),
                Email = Clean(input.Email),
                PhoneNumber = Clean(input.PhoneNumber),
                Case = input.Case,
                Score = input.Score,
                Summary = Clean(input.Summary),
                AddedByStaffId = addedByStaffId,
                RunId = runId,
                CreatedAt = DateTime.UtcNow,
                Evidence = input.Evidence
                    .Where(e => !string.IsNullOrWhiteSpace(e.Finding))
                    .Select(e => new ResearchLeadEvidence
                    {
                        Finding = e.Finding.Trim(),
                        SourceUrl = Clean(e.SourceUrl)
                    })
                    .ToList()
            };
        }

        private static void RequireNew(ResearchLead lead)
        {
            if (lead.Status == ResearchLeadStatus.New)
            {
                return;
            }

            throw new InvalidOperationException(lead.Status == ResearchLeadStatus.Accepted
                ? "This lead has already been accepted."
                : "This lead has already been dismissed.");
        }

        private static string BuildNotes(ResearchLead lead)
        {
            var notes = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(lead.Summary))
            {
                notes.AppendLine(lead.Summary);
                notes.AppendLine();
            }

            notes.AppendLine($"Why we flagged it: {CaseLabel(lead.Case)}");
            if (lead.Website is not null) notes.AppendLine($"Website: {lead.Website}");
            if (lead.Industry is not null) notes.AppendLine($"Industry: {lead.Industry}");
            if (lead.Location is not null) notes.AppendLine($"Location: {lead.Location}");

            if (lead.Evidence.Count > 0)
            {
                notes.AppendLine();
                notes.AppendLine("Evidence:");
                foreach (var item in lead.Evidence)
                {
                    notes.AppendLine(item.SourceUrl is null
                        ? $"- {item.Finding}"
                        : $"- {item.Finding} ({item.SourceUrl})");
                }
            }

            var text = notes.ToString().TrimEnd();
            return text.Length <= MaxNotesLength ? text : text[..(MaxNotesLength - 1)] + "…";
        }

        private static string CaseLabel(ResearchLeadCase leadCase) => leadCase switch
        {
            ResearchLeadCase.StrugglingPr => "Struggling PR",
            ResearchLeadCase.WeakSocialPresence => "Weak social media presence",
            ResearchLeadCase.NoWebsite => "No website",
            ResearchLeadCase.OutdatedWebsite => "Outdated website",
            _ => "Other"
        };

        private static ResearchLeadDto ToDto(ResearchLead lead) => new()
        {
            Id = lead.Id,
            CompanyName = lead.CompanyName,
            Website = lead.Website,
            Industry = lead.Industry,
            Location = lead.Location,
            Email = lead.Email,
            PhoneNumber = lead.PhoneNumber,
            Case = lead.Case,
            Score = lead.Score,
            Summary = lead.Summary,
            Status = lead.Status,
            DismissReason = lead.DismissReason,
            ReviewedByName = lead.ReviewedByStaff is null
                ? null
                : $"{lead.ReviewedByStaff.Name} {lead.ReviewedByStaff.Surname}".Trim(),
            ReviewedAt = lead.ReviewedAt,
            OutreachCompanyId = lead.OutreachCompanyId ?? lead.OutreachCompany?.Id,
            FoundBy = lead.AddedByStaff is null
                ? "Research agent"
                : $"{lead.AddedByStaff.Name} {lead.AddedByStaff.Surname}".Trim(),
            CreatedAt = lead.CreatedAt,
            Evidence = lead.Evidence
                .Select(e => new ResearchLeadEvidenceDto { Finding = e.Finding, SourceUrl = e.SourceUrl })
                .ToList()
        };

        private async Task<Staff> RequireStaffAsync(int staffId)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId);
            if (staff is null)
            {
                throw new UnauthorizedAccessException("Your session is no longer valid.");
            }

            return staff;
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
