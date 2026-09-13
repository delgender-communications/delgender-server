using Application.Services;
using Application.Validators;
using Core.Configuration;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using DelgenderCommunicationsAPI.Filters;
using DelgenderCommunicationsAPI.Middleware;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Text;
using System.Threading.RateLimiting;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    builder.Services.AddScoped<IBookingRepository, BookingRepository>();
    builder.Services.AddScoped<IConfirmationRepository, ConfirmationRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IStaffRepository, StaffRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<ITrustedDeviceRepository, TrustedDeviceRepository>();
    builder.Services.AddScoped<ILoginOtpRepository, LoginOtpRepository>();
    builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
    builder.Services.AddScoped<IPageViewRepository, PageViewRepository>();

    // Services
    builder.Services.AddOptions();
    builder.Services.AddHttpClient();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddSingleton<IResend>(sp =>
    {
        var apiKey = builder.Configuration["RESEND_API_KEY"]
            ?? throw new InvalidOperationException(
                "RESEND_API_KEY is not configured.");

        return ResendClient.Create(apiKey);
    });

    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IBookingService, BookingService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IStaffService, StaffService>();
    builder.Services.AddScoped<IInvoiceService, InvoiceService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

    builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    // Fire-and-forget queue for cheap reconciliation work (e.g. overdue invoices)
    // kicked off on login instead of a scheduler - see AuthService.EnqueueOverdueInvoiceSweep.
    builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
    builder.Services.AddHostedService<QueuedHostedService>();

    builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

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
    builder.Services.AddSwaggerGen();

    builder.Services.AddValidatorsFromAssemblyContaining<CreateBookingDtoValidator>();

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

        // Supports a single origin or a comma-separated list (public site + staff portal)
        var origins = allowedOrigin
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        options.AddPolicy("AllowFrontend", policy =>
            policy.WithOrigins(origins)
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // Build app
    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await Infrastructure.Data.AdminSeeder.SeedAsync(db, app.Configuration);

        // covers the gap while the app was asleep (Railway etc.) - the login-triggered
        // sweep in AuthService then keeps it current from here on
        var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        await invoiceService.RefreshOverdueInvoicesAsync();
    }

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
    Serilog.Log.CloseAndFlush();
}
