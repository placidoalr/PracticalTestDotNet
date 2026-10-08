using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using PracticalTestDotNet.Api.Endpoints;
using PracticalTestDotNet.Api.Middlewares;
using PracticalTestDotNet.Application;
using PracticalTestDotNet.Infrastructure;
using PracticalTestDotNet.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

// 3. Configure Layers (Clean Architecture DI)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 4. Configure OpenAPI & Scalar
builder.Services.AddOpenApi();

var app = builder.Build();

// 5. Apply Migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations.");
    }
}

// 6. Request Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

// 7. Map Endpoint Groups
app.MapAuthEndpoints();
app.MapOrderEndpoints();

try
{
    Log.Information("Starting PracticalTestDotNet API host...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

// Make public for WebApplicationFactory in Integration Tests
public partial class Program { }
