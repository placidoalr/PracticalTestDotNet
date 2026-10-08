namespace PracticalTestDotNet.Application.Dtos;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, string TokenType = "Bearer", int ExpiresInSeconds = 3600);
