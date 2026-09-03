using Core.Configuration;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using DelgenderCommunicationsAPI.Filters;
using DelgenderCommunicationsAPI.Middleware;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Application.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    builder.Services.AddScoped<IBookingRepository, BookingRepository>();
    builder.Services.AddScoped<IConfirmationRepository, ConfirmationRepository>();

    // Services
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IBookingService, BookingService>();

    // JWT Authentication
    // (add your JWT setup here)

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name
                              ?? context.Connection.RemoteIpAddress?.ToString()
                              ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        options.AddPolicy("booking", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name
                              ?? context.Connection.RemoteIpAddress?.ToString()
                              ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.ContentType = "application/problem+json";

            var response = new
            {
                type = "https://tools.ietf.org/html/rfc6585#section-4",
                title = "Too many requests",
                status = 429,
                detail = "Rate limit exceeded. Please try again shortly.",
                instance = context.HttpContext.Request.Path.Value
            };

            await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        };
    });

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"];

        if (string.IsNullOrWhiteSpace(allowedOrigin))
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigin is not configured.");
        }

        options.AddPolicy("AllowFrontend", policy =>
            policy.WithOrigins(allowedOrigin)
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // Build app
    var app = builder.Build();

    // Middleware pipeline
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
finally
{
    Log.CloseAndFlush();
}
