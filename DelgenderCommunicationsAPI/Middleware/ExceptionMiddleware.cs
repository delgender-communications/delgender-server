using System.Net;
using System.Text.Json;

namespace DelgenderCommunicationsAPI.Middleware
{
    /// <summary>
    /// Turns thrown exceptions into JSON error responses.
    ///
    /// The important bit: exceptions the services throw deliberately (bad password,
    /// expired OTP, resend cooldown, "already responded to", etc.) carry a message
    /// written for the person reading it, so that message is sent through as-is with
    /// a matching status code. Only genuinely unexpected exceptions get replaced with
    /// a generic message, since those can leak internals (stack traces, SQL, etc.).
    ///
    /// Adding a new user-facing error = throw one of the mapped exception types with
    /// the message you want shown. No middleware change needed.
    /// </summary>
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

                // Expected errors: send the real message, it's written for the user.
                // Unexpected: generic message, with the detail only in development.
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
            // Wrong password, expired/invalid OTP, deactivated account, expired session
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message, true),

            // Resend cooldown, booking already responded to, duplicate email,
            // editing a non-draft invoice
            InvalidOperationException => (HttpStatusCode.Conflict, ex.Message, true),

            // Missing record the caller asked for
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message, true),

            // Bad input that got past validation
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message, true),

            // Anything else is a bug - don't leak it
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", false)
        };
    }
}
