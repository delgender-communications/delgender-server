using System.Net;
using System.Text.Json;

namespace DelgenderCommunicationsAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _Next;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate Next, IHostEnvironment env)
        {
            _Next = Next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _Next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception: " + ex);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new { message = "An unexpected error occured.", detail = (string?)ex.Message }
                    : new { message = "An unexpected error occured.", detail = (string?)null };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
