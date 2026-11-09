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

                var (statusCode, message, isExpected) = Map(ex);
                context.Response.StatusCode = (int)statusCode;

                var response = new
                {
                    message,
                    detail = isExpected || _env.IsDevelopment() ? ex.Message : null
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        private static (HttpStatusCode StatusCode, string Message, bool IsExpected) Map(Exception ex) => ex switch
        {
            // wrong password, expired/invalid OTP, deactivated account, expired session
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message, true),

            // resend cooldown, booking already responded to, duplicate email, editing a non-draft invoice
            InvalidOperationException => (HttpStatusCode.Conflict, ex.Message, true),

            // missing record the caller asked for
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message, true),

            // bad input that got past validation
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message, true),

            // anything else is a bug and not leaked
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", false)
        };
    }
}
