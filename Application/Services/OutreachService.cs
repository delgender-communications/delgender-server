using Core.DTOs;
using Core.DTOs.Outreach;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Services
{
    public class OutreachService : IOutreachService
    {
        private readonly IOutreachRepository _outreachRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public OutreachService(
            IOutreachRepository outreachRepository,
            IStaffRepository staffRepository,
            ICloudinaryService cloudinaryService)
        {
            _outreachRepository = outreachRepository;
            _staffRepository = staffRepository;
            _cloudinaryService = cloudinaryService;
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

            return $"changed the status from {Label(from)} to Researching. The company is still being researched.";
        }

        private static string Label(OutreachStatus status) => status switch
        {
            OutreachStatus.Ready => "Ready",
            OutreachStatus.Emailed => "Emailed",
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
