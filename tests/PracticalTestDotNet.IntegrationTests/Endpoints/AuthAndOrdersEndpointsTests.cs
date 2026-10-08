using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Application.Orders.Commands.CreateOrder;
using Xunit;

namespace PracticalTestDotNet.IntegrationTests.Endpoints;

public class AuthAndOrdersEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthAndOrdersEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnJwtToken()
    {
        // Arrange
        var request = new LoginRequest("dev@martech.com", "Senha@123");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("invalid@martech.com", "wrongpassword");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), [new CreateOrderItemDto("Item", 1, 10m)]);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithValidToken_ShouldCreateOrderAndReturn21Created()
    {
        // 1. Login to get token
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequest("dev@martech.com", "Senha@123"));
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginContent!.Token);

        // 2. Post Order
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            [
                new CreateOrderItemDto("Laptop", 1, 3500.00m),
                new CreateOrderItemDto("Mouse", 1, 150.00m)
            ]
        );

        var response = await _client.PostAsJsonAsync("/api/orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.TotalAmount.Should().Be(3650.00m);
        order.Items.Should().HaveCount(2);
    }
}
