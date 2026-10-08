using FluentAssertions;
using NSubstitute;
using PracticalTestDotNet.Application.Orders.Queries.GetOrderById;
using PracticalTestDotNet.Application.Orders.Queries.GetOrdersPaginated;
using PracticalTestDotNet.Domain.Entities;
using PracticalTestDotNet.Domain.Interfaces;
using Xunit;

namespace PracticalTestDotNet.UnitTests.Handlers;

public class QueryHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task GetOrderByIdQueryHandler_WhenOrderExists_ShouldReturnOrderDto()
    {
        // Arrange
        var existingOrder = Order.Create(Guid.NewGuid(), [("Item 1", 1, 50m)]);
        _repository.GetByIdAsync(existingOrder.Id, Arg.Any<CancellationToken>())
            .Returns(existingOrder);

        var handler = new GetOrderByIdQueryHandler(_repository);
        var query = new GetOrderByIdQuery(existingOrder.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingOrder.Id);
        result.TotalAmount.Should().Be(50m);
    }

    [Fact]
    public async Task GetOrderByIdQueryHandler_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var handler = new GetOrderByIdQueryHandler(_repository);
        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrdersPaginatedQueryHandler_ShouldReturnPaginatedList()
    {
        // Arrange
        var order1 = Order.Create(Guid.NewGuid(), [("Item 1", 1, 10m)]);
        var order2 = Order.Create(Guid.NewGuid(), [("Item 2", 2, 20m)]);
        var list = new List<Order> { order1, order2 };

        _repository.GetPaginatedAsync(1, 10, Arg.Any<CancellationToken>())
            .Returns((list, 2));

        var handler = new GetOrdersPaginatedQueryHandler(_repository);
        var query = new GetOrdersPaginatedQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
