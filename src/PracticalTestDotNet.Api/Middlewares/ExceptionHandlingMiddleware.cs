using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PracticalTestDotNet.Domain.Exceptions;

namespace PracticalTestDotNet.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        ProblemDetails problemDetails;

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                problemDetails = new ValidationProblemDetails(errors)
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                };
                break;

            case DomainException domainException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Business Rule Violation",
                    Detail = domainException.Message,
                    Instance = context.Request.Path
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred on the server.",
                    Instance = context.Request.Path
                };
                break;
        }

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
