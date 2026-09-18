using Core.DTOs;
using Core.DTOs.Client;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services
{
    public class ClientService : IClientService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public ClientService(
            ICustomerRepository customerRepository,
            IFeedbackRepository feedbackRepository,
            IBackgroundTaskQueue backgroundTaskQueue)
        {
            _customerRepository = customerRepository;
            _feedbackRepository = feedbackRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public async Task<PagedResultDto<ClientDto>> GetAllAsync(int page, int pageSize, ClientStatus? status)
        {
            var clients = await _customerRepository.GetClientsAsync(page, pageSize, status);
            var total = await _customerRepository.GetClientsCountAsync(status);

            return new PagedResultDto<ClientDto>
            {
                Data = clients.Select(ToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ClientDto> CreateAsync(CreateClientDto dto)
        {
            var existing = await _customerRepository.GetByEmailAsync(dto.Email);
            if (existing is not null)
            {
                throw new InvalidOperationException("A client with this email already exists.");
            }

            var customer = new Customer
            {
                FullName = dto.FullName,
                JobTitle = dto.JobTitle,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Industry = dto.Industry,
                ContactPermission = dto.ContactPermission,
                Status = dto.Status,
                WorkingSince = dto.Status == ClientStatus.Working ? DateTime.UtcNow : null
            };

            customer = await _customerRepository.CreateAsync(customer);
            return ToDto(customer);
        }

        public async Task<ClientDto?> UpdateStatusAsync(int id, UpdateClientStatusDto dto, int staffId)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer is null)
            {
                return null;
            }

            var previousStatus = customer.Status;

            if (dto.Status == ClientStatus.Working && previousStatus != ClientStatus.Working)
            {
                customer.WorkingSince = DateTime.UtcNow;
                customer.WorkingUntil = null;
            }
            else if (previousStatus == ClientStatus.Working && dto.Status != ClientStatus.Working)
            {
                customer.WorkingUntil = DateTime.UtcNow;

                if (dto.RequestFeedback)
                {
                    var feedback = await _feedbackRepository.CreateAsync(new Feedback
                    {
                        CustomerId = customer.Id,
                        RequestedByStaffId = staffId
                    });

                    var customerId = customer.Id;

                    _backgroundTaskQueue.Enqueue(async (services, cancellationToken) =>
                    {
                        var customerRepository = services.GetRequiredService<ICustomerRepository>();
                        var emailService = services.GetRequiredService<IEmailService>();

                        var freshCustomer = await customerRepository.GetByIdAsync(customerId);
                        if (freshCustomer is null) return;

                        await emailService.SendFeedbackRequestAsync(freshCustomer);
                    });
                }
            }

            customer.Status = dto.Status;
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);
            return ToDto(customer);
        }

        private static ClientDto ToDto(Customer customer) => new()
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName,
            ContactFullName = customer.FullName,
            ContactJobTitle = customer.JobTitle,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Industry = customer.Industry,
            Status = customer.Status,
            WorkingSince = customer.WorkingSince,
            WorkingUntil = customer.WorkingUntil,
            CreatedAt = customer.CreatedAt
        };
    }
}
