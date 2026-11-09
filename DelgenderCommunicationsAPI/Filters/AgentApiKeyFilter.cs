using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace DelgenderCommunicationsAPI.Filters
{
    // lets the research agent in with a shared secret instead of a staff login
    public class AgentApiKeyFilter : IAsyncAuthorizationFilter
    {
        private const string HeaderName = "X-Api-Key";
        private const int MinKeyLength = 24;

        private readonly IConfiguration _configuration;

        public AgentApiKeyFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var expected = _configuration["ResearchAgent:ApiKey"];
            if (string.IsNullOrWhiteSpace(expected) || expected.Length < MinKeyLength)
            {
                context.Result = new ObjectResult(new ProblemDetails
                {
                    Title = "The research agent isn't set up on this server.",
                    Status = StatusCodes.Status503ServiceUnavailable
                })
                { StatusCode = StatusCodes.Status503ServiceUnavailable };
                return Task.CompletedTask;
            }

            var provided = context.HttpContext.Request.Headers[HeaderName].ToString();

            var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
            var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(provided));

            if (!CryptographicOperations.FixedTimeEquals(expectedHash, providedHash))
            {
                context.Result = new UnauthorizedResult();
            }

            return Task.CompletedTask;
        }
    }
}
