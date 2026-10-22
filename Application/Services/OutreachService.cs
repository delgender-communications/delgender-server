using Core.DTOs;
using Core.DTOs.Outreach;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using System.Collections.Concurrent;

namespace Application.Services
{
    public class OutreachService : IOutreachService
    {
        private readonly IOutreachRepository _outreachRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IPitchDeckFetcher _pitchDeckFetcher;
        private readonly IOutreachMailer _outreachMailer;

        // Companies whose email is being sent right now. Stops a double click (or two
        // teammates clicking at once) from emailing the same company twice.
        private static readonly ConcurrentDictionary<int, byte> SendsInProgress = new();

        public OutreachService(
            IOutreachRepository outreachRepository,
            IStaffRepository staffRepository,
            ICloudinaryService cloudinaryService,
            IPitchDeckFetcher pitchDeckFetcher,
            IOutreachMailer outreachMailer)
        {
            _outreachRepository = outreachRepository;
            _staffRepository = staffRepository;
            _cloudinaryService = cloudinaryService;
            _pitchDeckFetcher = pitchDeckFetcher;
            _outreachMailer = outreachMailer;
        }

        public async Task<PagedResultDto<OutreachListItemDto>> GetAllAsync(int page, int pageSize, OutreachStatus? status)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 15;

            var companies = await _outreachRepository.GetPageAsync(page, pageSize, status);
            var total = await _outreachRepository.CountAsync(status);

            return new PagedResultDto<OutreachListItemDto>
            {
                Data = companies.Select(ToListItem),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<OutreachDetailDto?> GetByIdAsync(int id)
        {
            var company = await _outreachRepository.GetByIdWithDetailsAsync(id);
            return company is null ? null : ToDetail(company);
        }

        public async Task<OutreachDetailDto> CreateAsync(CreateOutreachDto dto, int staffId)
        {
            var staff = await RequireStaffAsync(staffId);
            var now = DateTime.UtcNow;

            var company = new OutreachCompany
            {
                CompanyName = dto.CompanyName.Trim(),
                ContactName = Clean(dto.ContactName),
                Email = Clean(dto.Email),
                PhoneNumber = Clean(dto.PhoneNumber),
                Notes = Clean(dto.Notes),
                Status = dto.Status,
                AddedByStaffId = staff.Id,
                CreatedAt = now,
                UpdatedAt = now
            };

            company.Logs.Add(NewLog(staff, AddedMessage(dto.Status), now));

            company = await _outreachRepository.CreateAsync(company);
            company.AddedByStaff = staff;
            return ToDetail(company);
        }

        public async Task<OutreachDetailDto?> UpdateAsync(int id, UpdateOutreachDto dto, int staffId)
        {
            var company = await _outreachRepository.GetByIdWithDetailsAsync(id);
            if (company is null)
            {
                return null;
            }

            var staff = await RequireStaffAsync(staffId);
            var now = DateTime.UtcNow;
            var changed = false;

            var companyName = dto.CompanyName.Trim();
            if (!string.Equals(company.CompanyName, companyName, StringComparison.Ordinal))
            {
                company.Logs.Add(NewLog(
                    staff,
                    $"updated the company name from \"{company.CompanyName}\" to \"{companyName}\".",
                    now));
                company.CompanyName = companyName;
                changed = true;
            }

            changed |= ApplyOptional(company, staff, now, "contact", company.ContactName, Clean(dto.ContactName), value => company.ContactName = value);
            changed |= ApplyOptional(company, staff, now, "email", company.Email, Clean(dto.Email), value => company.Email = value);
            changed |= ApplyOptional(company, staff, now, "phone number", company.PhoneNumber, Clean(dto.PhoneNumber), value => company.PhoneNumber = value);

            var notes = Clean(dto.Notes);
            if (!string.Equals(company.Notes, notes, StringComparison.Ordinal))
            {
                company.Notes = notes;
                changed = true;
            }

            if (company.Status != dto.Status)
            {
                company.Logs.Add(NewLog(staff, StatusChangeMessage(company.Status, dto.Status), now));
                company.Status = dto.Status;
                changed = true;
            }

            if (changed)
            {
                company.UpdatedAt = now;
                await _outreachRepository.SaveChangesAsync();
            }

            return ToDetail(company);
        }

        public async Task<OutreachDetailDto?> UpdateNotesAsync(int id, UpdateOutreachNotesDto dto)
        {
            var company = await _outreachRepository.GetByIdWithDetailsAsync(id);
            if (company is null)
            {
                return null;
            }

            var notes = Clean(dto.Notes);
            if (!string.Equals(company.Notes, notes, StringComparison.Ordinal))
            {
                company.Notes = notes;
                company.UpdatedAt = DateTime.UtcNow;
                await _outreachRepository.SaveChangesAsync();
            }

            return ToDetail(company);
        }

        public async Task<OutreachDetailDto?> UploadPitchDeckAsync(int id, Stream fileStream, string fileName, int staffId)
        {
            var company = await _outreachRepository.GetByIdWithDetailsAsync(id);
            if (company is null)
            {
                return null;
            }

            var staff = await RequireStaffAsync(staffId);
            var safeName = Path.GetFileName(fileName);
            var (url, publicId) = await _cloudinaryService.UploadRawAsync(
                fileStream,
                safeName,
                "delgender/outreach-decks");

            var previousName = company.PitchDeckFileName;
            var previousPublicId = company.PitchDeckPublicId;
            var now = DateTime.UtcNow;

            var message = string.IsNullOrWhiteSpace(previousName)
                ? $"uploaded the pitch deck \"{safeName}\"."
                : $"replaced the pitch deck. The previous file was \"{previousName}\" and the new file is \"{safeName}\".";

            company.PitchDeckUrl = url;
            company.PitchDeckPublicId = publicId;
            company.PitchDeckFileName = safeName;
            company.PitchDeckUploadedAt = now;
            company.UpdatedAt = now;
            company.Logs.Add(NewLog(staff, message, now));

            await _outreachRepository.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(previousPublicId))
            {
                await _cloudinaryService.DeleteRawAsync(previousPublicId);
            }

            return ToDetail(company);
        }

        public async Task<(byte[] Bytes, string FileName)?> DownloadPitchDeckAsync(int id)
        {
            var company = await _outreachRepository.GetByIdWithDetailsAsync(id);
            if (company is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(company.PitchDeckUrl))
            {
                throw new KeyNotFoundException("This company doesn't have a pitch deck yet.");
            }

            var bytes = await _pitchDeckFetcher.FetchAsync(DeckSourceUrl(company));
            return (bytes, DeckFileName(company));
        }

        /// <summary>
        /// The "email it for me" pipeline:
        ///   1. lock the company so it can't be sent twice at once
        ///   2. check it is Ready and has an email address and a pitch deck
        ///   3. download the pitch deck
        ///   4. send the email (text lives in OutreachEmailContent)
        ///   5. mark the company Emailed and write it to the activity log
        /// The status only changes after the email has actually gone out, so a failed
        /// send leaves the company Ready and nothing is logged.
        /// </summary>
        public async Task<OutreachDetailDto?> SendEmailAsync(int id, int staffId)
        {
            if (!SendsInProgress.TryAdd(id, 0))
            {
                throw new InvalidOperationException("This email is already being sent. Give it a moment.");
            }

            try
            {
                var company = await _outreachRepository.GetByIdWithDetailsAsync(id);
                if (company is null)
                {
                    return null;
                }

                var staff = await RequireStaffAsync(staffId);

                RequireReady(company);

                if (string.IsNullOrWhiteSpace(company.Email))
                {
                    throw new InvalidOperationException("Add an email address for this company before sending.");
                }

                if (string.IsNullOrWhiteSpace(company.PitchDeckUrl))
                {
                    throw new InvalidOperationException("Upload the pitch deck before sending.");
                }

                var deck = await _pitchDeckFetcher.FetchAsync(DeckSourceUrl(company));

                await _outreachMailer.SendPitchAsync(new OutreachPitchEmail(
                    RecipientEmail: company.Email,
                    RecipientName: company.ContactName,
                    CompanyName: company.CompanyName,
                    SenderName: $"{staff.Name} {staff.Surname}".Trim(),
                    PitchDeckBytes: deck,
                    PitchDeckFileName: DeckFileName(company)));

                var now = DateTime.UtcNow;
                company.Status = OutreachStatus.Emailed;
                company.UpdatedAt = now;
                company.Logs.Add(NewLog(
                    staff,
                    $"emailed the pitch deck to {company.Email} through the system. Everything is done.",
                    now));

                try
                {
                    await _outreachRepository.SaveChangesAsync();
                }
                catch
                {
                    // The email is already out, so don't let anyone retry blindly.
                    throw new InvalidOperationException(
                        "The email was sent, but we couldn't update the status. Please mark it as Emailed yourself so nobody sends it twice.");
                }

                return ToDetail(company);
            }
            finally
            {
                SendsInProgress.TryRemove(id, out _);
            }
        }

        public async Task<OutreachDetailDto?> MarkEmailedAsync(int id, int staffId)
        {
            var company = await _outreachRepository.GetByIdWithDetailsAsync(id);
            if (company is null)
            {
                return null;
            }

            var staff = await RequireStaffAsync(staffId);

            RequireReady(company);

            var now = DateTime.UtcNow;
            company.Status = OutreachStatus.Emailed;
            company.UpdatedAt = now;
            company.Logs.Add(NewLog(
                staff,
                "marked this as Emailed. They sent the email themselves, outside the system. Everything is done.",
                now));

            await _outreachRepository.SaveChangesAsync();
            return ToDetail(company);
        }

        // Prefer a signed link so Cloudinary's "block PDF delivery" setting can't get
        // in the way. Falls back to the stored URL for decks without a public id.
        private string DeckSourceUrl(OutreachCompany company) =>
            string.IsNullOrWhiteSpace(company.PitchDeckPublicId)
                ? company.PitchDeckUrl!
                : _cloudinaryService.GetRawDownloadUrl(company.PitchDeckPublicId);

        private static void RequireReady(OutreachCompany company)
        {
            if (company.Status == OutreachStatus.Ready)
            {
                return;
            }

            throw new InvalidOperationException(company.Status switch
            {
                OutreachStatus.Emailed => "This company has already been marked as Emailed.",
                OutreachStatus.Cancelled => "This company was cancelled, so it can't be emailed.",
                _ => "Only companies marked Ready can be emailed."
            });
        }

        private static string DeckFileName(OutreachCompany company)
        {
            var name = Path.GetFileName(company.PitchDeckFileName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
            {
                var invalid = Path.GetInvalidFileNameChars();
                var safeCompany = new string(company.CompanyName.Where(c => !invalid.Contains(c)).ToArray()).Trim();
                name = $"{(safeCompany.Length == 0 ? "Pitch deck" : safeCompany)} pitch deck";
            }

            return name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? name : $"{name}.pdf";
        }

        private async Task<Staff> RequireStaffAsync(int staffId)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId);
            if (staff is null)
            {
                throw new UnauthorizedAccessException("Your session is no longer valid.");
            }

            return staff;
        }

        private static bool ApplyOptional(
            OutreachCompany company,
            Staff staff,
            DateTime now,
            string field,
            string? current,
            string? next,
            Action<string?> assign)
        {
            if (string.Equals(current, next, StringComparison.Ordinal))
            {
                return false;
            }

            company.Logs.Add(NewLog(staff, DescribeOptionalChange(field, current, next), now));
            assign(next);
            return true;
        }

        private static string DescribeOptionalChange(string field, string? from, string? to)
        {
            if (from is null)
            {
                return $"added the {field} \"{to}\".";
            }

            if (to is null)
            {
                return $"cleared the {field}.";
            }

            return $"updated the {field} from \"{from}\" to \"{to}\".";
        }

        private static string AddedMessage(OutreachStatus status) => status switch
        {
            OutreachStatus.Ready => "added this company as Ready. The pitch can be downloaded and emailed.",
            OutreachStatus.Emailed => "added this company as Emailed. This outreach is already done.",
            OutreachStatus.Cancelled => "added this company as Cancelled. We are not pursuing it.",
            _ => "added this company. It is still being researched."
        };

        private static string StatusChangeMessage(OutreachStatus from, OutreachStatus to)
        {
            if (to == OutreachStatus.Ready)
            {
                return $"changed the status from {Label(from)} to Ready. Everything is ready, so anyone can download the pitch deck and email the company.";
            }

            if (to == OutreachStatus.Emailed)
            {
                return $"changed the status from {Label(from)} to Emailed. Everything is done.";
            }

            if (to == OutreachStatus.Cancelled)
            {
                return $"changed the status from {Label(from)} to Cancelled. We decided not to pursue this company.";
            }

            return $"changed the status from {Label(from)} to Researching. The company is still being researched.";
        }

        private static string Label(OutreachStatus status) => status switch
        {
            OutreachStatus.Ready => "Ready",
            OutreachStatus.Emailed => "Emailed",
            OutreachStatus.Cancelled => "Cancelled",
            _ => "Researching"
        };

        private static OutreachLog NewLog(Staff staff, string message, DateTime at) => new()
        {
            StaffId = staff.Id,
            ActorName = $"{staff.Name} {staff.Surname}".Trim(),
            Message = message,
            CreatedAt = at
        };

        private static string? Clean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private static OutreachListItemDto ToListItem(OutreachCompany company) => new()
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            ContactName = company.ContactName,
            Email = company.Email,
            Status = company.Status,
            PitchDeckFileName = company.PitchDeckFileName,
            PitchDeckUrl = company.PitchDeckUrl,
            CreatedAt = company.CreatedAt
        };

        private static OutreachDetailDto ToDetail(OutreachCompany company) => new()
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            ContactName = company.ContactName,
            Email = company.Email,
            PhoneNumber = company.PhoneNumber,
            Notes = company.Notes,
            Status = company.Status,
            PitchDeckFileName = company.PitchDeckFileName,
            PitchDeckUrl = company.PitchDeckUrl,
            PitchDeckUploadedAt = company.PitchDeckUploadedAt,
            AddedByStaffId = company.AddedByStaffId,
            AddedByName = company.AddedByStaff is null
                ? "A teammate"
                : $"{company.AddedByStaff.Name} {company.AddedByStaff.Surname}".Trim(),
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            Logs = company.Logs
                .OrderByDescending(l => l.CreatedAt)
                .ThenByDescending(l => l.Id)
                .Select(l => new OutreachLogDto
                {
                    Id = l.Id,
                    ActorName = l.ActorName,
                    Message = l.Message,
                    CreatedAt = l.CreatedAt
                })
                .ToList()
        };
    }
}
