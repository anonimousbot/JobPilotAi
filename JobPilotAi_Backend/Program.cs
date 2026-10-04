using System.Reflection;
using JobPilotAi_Backend.Core;
using JobPilotAi_Backend.Core.Logging;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Features.Admin;
using JobPilotAi_Backend.Features.Analyses;
using JobPilotAi_Backend.Features.CoverLetters;
using JobPilotAi_Backend.Features.Identity.Auth;
using JobPilotAi_Backend.Features.Identity.Register;
using JobPilotAi_Backend.Features.JobMatches;
using JobPilotAi_Backend.Features.Profiles;
using JobPilotAi_Backend.Features.Resumes;
using JobPilotAi_Backend.Features.Subscriptions;
using JobPilotAi_Backend.Features.System.Health;
using JobPilotAi_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "JobPilotAi_Backend"), preserveStaticLogger: true);

    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "JobPilotAi Backend API",
            Version = "v1",
            Description = "MVP API for resume upload, ATS analysis, cover letters, subscriptions, and administration."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter the access token returned from /api/auth/login or /api/auth/refresh."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference("Bearer", document, null)
            ] = []
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    builder.Services.AddExceptionHandler<JobPilotAi_Backend.Core.Api.GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddCors(options =>
    {
        var configuredOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];
        var allowedOrigins = new[]
            {
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:51151",
                "http://localhost:51152"
            }
            .Concat(configuredOrigins)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        options.AddPolicy("Frontend", policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
    });
    builder.Services.AddValidators();
    builder.Services.AddApplicationCore(builder.Configuration);
    builder.Services.AddApplicationSecurity(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddRegisterUserFeature();
    builder.Services.AddAuthorization();

    var app = builder.Build();

    var dbOptions = app.Services.GetRequiredService<DatabaseConnectionOptions>();
    if (dbOptions.IsConfigured)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    var enableSwagger = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger", true);
    if (enableSwagger)
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "JobPilotAi Backend API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "JobPilotAi API";
            options.DisplayRequestDuration();
        });
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });

    app.UseExceptionHandler();

    app.UseMiddleware<RequestLogContextMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, _, exception) =>
            exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
                ? LogEventLevel.Error
                : LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
            diagnosticContext.Set("EndpointName", httpContext.GetEndpoint()?.DisplayName ?? "Unknown");

            if (httpContext.Items.TryGetValue(nameof(AuthContext), out var value) && value is AuthContext authContext)
            {
                diagnosticContext.Set("UserId", authContext.UserId);
                diagnosticContext.Set("UserRole", authContext.Role.ToString());
            }
        };
    });
    app.UseCors("Frontend");
    app.UseAuthorization();

    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.MapRegisterUserFeature();
    app.MapAuthFeature();
    app.MapProfileFeature();
    app.MapResumeFeature();
    app.MapAnalysisFeature();
    app.MapCoverLetterFeature();
    app.MapJobMatchFeature();
    app.MapSubscriptionFeature();
    app.MapAdminFeature();
    app.MapHealthFeature();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "JobPilotAi backend terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
