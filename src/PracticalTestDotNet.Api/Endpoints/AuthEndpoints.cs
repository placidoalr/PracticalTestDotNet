using Microsoft.AspNetCore.Mvc;
using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Application.Interfaces;

namespace PracticalTestDotNet.Api.Endpoints;

public static class AuthEndpoints
{
    private const string FixedEmail = "dev@martech.com";
    private const string FixedPassword = "Senha@123";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/login", (
            [FromBody] LoginRequest request,
            [FromServices] IJwtTokenGenerator tokenGenerator) =>
        {
            if (request.Email != FixedEmail || request.Password != FixedPassword)
            {
                return Results.Unauthorized();
            }

            var token = tokenGenerator.GenerateToken(Guid.NewGuid().ToString(), request.Email);
            return Results.Ok(new LoginResponse(token));
        })
        .WithName("Login")
        .WithSummary("Autentica o usuário dev@martech.com e retorna token JWT")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        return app;
    }
}
