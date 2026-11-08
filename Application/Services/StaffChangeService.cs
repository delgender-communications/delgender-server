using Core.DTOs.StaffRequests;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Utilities;

namespace Application.Services
{
    public class StaffChangeService : IStaffChangeService
    {
        private const int MyRequestsShown = 20;
        private const int ReviewListShown = 100;
        private const int MaxNoteLength = 500;

        private readonly IStaffChangeRequestRepository _requestRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly INotificationService _notificationService;

        public StaffChangeService(
            IStaffChangeRequestRepository requestRepository,
            IStaffRepository staffRepository,
            INotificationService notificationService)
        {
            _requestRepository = requestRepository;
            _staffRepository = staffRepository;
            _notificationService = notificationService;
        }

        public async Task<StaffChangeRequestDto> CreateAsync(int staffId, CreateStaffChangeRequestDto dto)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Staff member not found.");

            var request = new StaffChangeRequest
            {
                StaffId = staff.Id,
                Staff = staff,
                Type = dto.Type
            };

            switch (dto.Type)
            {
                case ChangeRequestType.Name:
                    {
                        var name = dto.NewValue.Trim();
                        var surname = (dto.NewSurname ?? string.Empty).Trim();

                        if (name.Length == 0 || surname.Length == 0)
                        {
                            throw new ArgumentException("Enter both your first name and your surname.");
                        }

                        if (SameText(name, staff.Name) && SameText(surname, staff.Surname))
                        {
                            throw new InvalidOperationException("That's already your name.");
                        }

                        request.OldValue = $"{staff.Name} {staff.Surname}";
                        request.NewValue = name;
                        request.NewSurname = surname;
                        break;
                    }

                case ChangeRequestType.Email:
                    {
                        var email = dto.NewValue.Trim();

                        if (SameText(email, staff.Email))
                        {
                            throw new InvalidOperationException("That's already your email.");
                        }

                        if (await _staffRepository.GetByEmailAsync(email) is not null)
                        {
                            throw new InvalidOperationException("Another staff member already uses that email.");
                        }

                        request.OldValue = staff.Email;
                        request.NewValue = email;
                        break;
                    }

                case ChangeRequestType.PhoneNumber:
                    {
                        if (!PhoneNumbers.TryNormalize(dto.NewValue, out var phone))
                        {
                            throw new ArgumentException("Enter a 10-digit phone number, like 081 234 1234.");
                        }

                        if (phone == PhoneNumbers.Normalize(staff.PhoneNumber))
                        {
                            throw new InvalidOperationException("That's already your phone number.");
                        }

                        request.OldValue = staff.PhoneNumber;
                        request.NewValue = phone;
                        break;
                    }

                default:
                    throw new ArgumentException("That kind of change isn't supported.");
            }

            if (await _requestRepository.HasPendingAsync(staff.Id, dto.Type))
            {
                throw new InvalidOperationException(
                    "You already have a request waiting for approval. Cancel it first if you want to send a different one.");
            }

            request = await _requestRepository.CreateAsync(request);

            await _notificationService.NotifyAdminsAsync(
                NotificationType.ChangeRequest,
                $"{staff.Name} {staff.Surname} asked to change their {Label(dto.Type)}",
                $"{request.OldValue} → {ToText(request)}",
                "/staff");

            return ToDto(request);
        }

        public async Task<List<StaffChangeRequestDto>> GetMineAsync(int staffId) =>
            (await _requestRepository.GetForStaffAsync(staffId, MyRequestsShown))
                .Select(ToDto)
                .ToList();

        public async Task<bool> CancelAsync(int staffId, int requestId)
        {
            var request = await _requestRepository.GetByIdWithPeopleAsync(requestId);
            if (request is null || request.StaffId != staffId) return false;

            if (request.Status != ChangeRequestStatus.Pending)
            {
                throw new InvalidOperationException("Only a request that's still waiting can be cancelled.");
            }

            request.Status = ChangeRequestStatus.Cancelled;
            await _requestRepository.SaveAsync();
            return true;
        }

        public async Task<List<StaffChangeRequestDto>> GetByStatusAsync(ChangeRequestStatus? status) =>
            (await _requestRepository.GetByStatusAsync(status, ReviewListShown))
                .Select(ToDto)
                .ToList();

        public Task<StaffChangeRequestDto> ApproveAsync(int adminId, int requestId, string? note) =>
            ReviewAsync(adminId, requestId, approve: true, note);

        public Task<StaffChangeRequestDto> RejectAsync(int adminId, int requestId, string? note) =>
            ReviewAsync(adminId, requestId, approve: false, note);

        private async Task<StaffChangeRequestDto> ReviewAsync(int adminId, int requestId, bool approve, string? note)
        {
            var request = await _requestRepository.GetByIdWithPeopleAsync(requestId)
                ?? throw new KeyNotFoundException("That request couldn't be found.");

            if (request.Status != ChangeRequestStatus.Pending)
            {
                throw new InvalidOperationException("This request has already been dealt with.");
            }

            var admin = await _staffRepository.GetByIdAsync(adminId)
                ?? throw new KeyNotFoundException("Staff member not found.");

            if (approve)
            {
                await ApplyAsync(request);
            }

            request.Status = approve ? ChangeRequestStatus.Approved : ChangeRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByStaffId = admin.Id;
            request.ReviewedByStaff = admin;

            var trimmed = note?.Trim();
            request.ReviewNote = string.IsNullOrEmpty(trimmed)
                ? null
                : trimmed[..Math.Min(trimmed.Length, MaxNoteLength)];

            // Saves the request and, when approved, the change to the staff member.
            await _requestRepository.SaveAsync();

            await _notificationService.NotifyStaffAsync(
                request.StaffId,
                NotificationType.ChangeRequestDecision,
                $"Your {Label(request.Type)} change was {(approve ? "approved" : "declined")}",
                request.ReviewNote ?? (approve ? $"It's now {ToText(request)}." : null),
                "/settings");

            return ToDto(request);
        }

        private async Task ApplyAsync(StaffChangeRequest request)
        {
            var staff = request.Staff;

            switch (request.Type)
            {
                case ChangeRequestType.Name:
                    staff.Name = request.NewValue;
                    staff.Surname = request.NewSurname ?? staff.Surname;
                    break;

                case ChangeRequestType.Email:
                    // Checked again: someone else may have taken it since it was asked for.
                    var existing = await _staffRepository.GetByEmailAsync(request.NewValue);
                    if (existing is not null && existing.Id != staff.Id)
                    {
                        throw new InvalidOperationException(
                            "Another staff member now uses that email, so this can't be approved.");
                    }
                    staff.Email = request.NewValue;
                    break;

                case ChangeRequestType.PhoneNumber:
                    staff.PhoneNumber = request.NewValue;
                    break;
            }

            staff.UpdatedAt = DateTime.UtcNow;
        }

        private static bool SameText(string a, string b) =>
            string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

        private static string Label(ChangeRequestType type) => type switch
        {
            ChangeRequestType.Name => "name",
            ChangeRequestType.Email => "email",
            _ => "phone number"
        };

        private static string ToText(StaffChangeRequest request) =>
            request.Type == ChangeRequestType.Name
                ? $"{request.NewValue} {request.NewSurname}".Trim()
                : request.NewValue;

        private static StaffChangeRequestDto ToDto(StaffChangeRequest request) => new()
        {
            Id = request.Id,
            StaffId = request.StaffId,
            StaffName = $"{request.Staff.Name} {request.Staff.Surname}",
            StaffCode = request.Staff.StaffId,
            Type = request.Type,
            From = request.OldValue,
            To = ToText(request),
            Status = request.Status,
            CreatedAt = request.CreatedAt,
            ReviewedAt = request.ReviewedAt,
            ReviewedByName = request.ReviewedByStaff is null
                ? null
                : $"{request.ReviewedByStaff.Name} {request.ReviewedByStaff.Surname}",
            ReviewNote = request.ReviewNote
        };
    }
}
