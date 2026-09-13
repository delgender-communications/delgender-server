using Core.DTOs;
using Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/customers")]
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomersController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        // Small, staff-only lookup used to attach an invoice to an existing customer
        // (one is created automatically whenever someone submits a booking).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll([FromQuery] string? search = null)
        {
            var customers = await _customerRepository.GetAllAsync();

            var query = customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    c.CompanyName.ToLower().Contains(term));
            }

            var result = query
                .OrderBy(c => c.FullName)
                .Take(50)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    JobTitle = c.JobTitle,
                    CompanyName = c.CompanyName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Industry = c.Industry,
                    IdNumber = c.IdNumber
                });

            return Ok(result);
        }
    }
}
