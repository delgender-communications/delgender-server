using Core.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class PhoneNumberBackfill
    {
        public static async Task RunAsync(AppDbContext db)
        {
            var changed = 0;
            var unreadable = 0;

            string? Fix(string? current)
            {
                if (string.IsNullOrWhiteSpace(current)) return current;

                if (!PhoneNumbers.TryNormalize(current, out var normalized))
                {
                    unreadable++;
                    return current;
                }

                if (normalized != current) changed++;
                return normalized;
            }

            foreach (var staff in await db.Staffs.ToListAsync())
            {
                staff.PhoneNumber = Fix(staff.PhoneNumber) ?? staff.PhoneNumber;
            }

            foreach (var customer in await db.Customers.ToListAsync())
            {
                customer.PhoneNumber = Fix(customer.PhoneNumber) ?? customer.PhoneNumber;
            }

            foreach (var company in await db.OutreachCompanies.ToListAsync())
            {
                company.PhoneNumber = Fix(company.PhoneNumber);
            }

            foreach (var lead in await db.ResearchLeads.ToListAsync())
            {
                lead.PhoneNumber = Fix(lead.PhoneNumber);
            }

            if (changed > 0)
            {
                await db.SaveChangesAsync();
                Console.WriteLine($"Phone numbers: {changed} reformatted.");
            }

            if (unreadable > 0)
            {
                Console.WriteLine($"Phone numbers: {unreadable} don't look like 10-digit South African numbers and were left as they are.");
            }
        }
    }
}
