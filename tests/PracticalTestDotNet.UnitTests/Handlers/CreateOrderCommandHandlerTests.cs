using FluentAssertions;
using NSubstitute;
using PracticalTestDotNet.Application.Orders.Commands.CreateOrder;
using PracticalTestDotNet.Domain.Entities;
using PracticalTestDotNet.Domain.Interfaces;
using Xunit;

namespace PracticalTestDotNet.UnitTests.Handlers;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddOrderToRepositoryAndReturnDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<CreateOrderItemDto>
        {
            new("Keyboard", 1, 150.00m),
            new("Mouse", 2, 50.00m)
        };
        var command = new CreateOrderCommand(customerId, items);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be("Pending");
        result.Items.Should().HaveCount(2);
        result.TotalAmount.Should().Be(250.00m);

        await _repository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
