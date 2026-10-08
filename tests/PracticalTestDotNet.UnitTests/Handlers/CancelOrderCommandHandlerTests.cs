using FluentAssertions;
using NSubstitute;
using PracticalTestDotNet.Application.Orders.Commands.CancelOrder;
using PracticalTestDotNet.Domain.Entities;
using PracticalTestDotNet.Domain.Exceptions;
using PracticalTestDotNet.Domain.Interfaces;
using Xunit;

namespace PracticalTestDotNet.UnitTests.Handlers;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _handler = new CancelOrderCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingPendingOrder_ShouldCancelOrderAndUpdateRepository()
    {
        // Arrange
        var existingOrder = Order.Create(Guid.NewGuid(), [("Item 1", 1, 100m)]);
        _repository.GetByIdAsync(existingOrder.Id, Arg.Any<CancellationToken>())
            .Returns(existingOrder);

        var command = new CancelOrderCommand(existingOrder.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Cancelled");

        await _repository.Received(1).UpdateAsync(existingOrder, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingOrder_ShouldThrowDomainException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var command = new CancelOrderCommand(orderId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"Order with ID '{orderId}' was not found.");
    }
}
