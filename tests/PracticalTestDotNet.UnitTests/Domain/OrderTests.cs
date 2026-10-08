using FluentAssertions;
using PracticalTestDotNet.Domain.Entities;
using PracticalTestDotNet.Domain.Enums;
using PracticalTestDotNet.Domain.Exceptions;
using Xunit;

namespace PracticalTestDotNet.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateOrderAndCalculateTotalAmountInDomain()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<(string ProductName, int Quantity, decimal UnitPrice)>
        {
            ("Product A", 2, 10.50m), // Total = 21.00
            ("Product B", 1, 15.00m)  // Total = 15.00
        };

        // Act
        var order = Order.Create(customerId, items);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().HaveCount(2);
        order.TotalAmount.Should().Be(36.00m); // Calculated inside domain entity
    }

    [Fact]
    public void Create_WithoutItems_ShouldThrowDomainException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var emptyItems = new List<(string ProductName, int Quantity, decimal UnitPrice)>();

        // Act
        Action act = () => Order.Create(customerId, emptyItems);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("An order must contain at least one item.");
    }

    [Theory]
    [InlineData(0, 10.0)]
    [InlineData(-1, 10.0)]
    public void Create_WithInvalidQuantity_ShouldThrowDomainException(int quantity, decimal price)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<(string ProductName, int Quantity, decimal UnitPrice)>
        {
            ("Product A", quantity, price)
        };

        // Act
        Action act = () => Order.Create(customerId, items);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Quantity must be greater than zero.");
    }

    [Theory]
    [InlineData(1, 0.0)]
    [InlineData(1, -5.0)]
    public void Create_WithInvalidUnitPrice_ShouldThrowDomainException(int quantity, decimal price)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<(string ProductName, int Quantity, decimal UnitPrice)>
        {
            ("Product A", quantity, price)
        };

        // Act
        Action act = () => Order.Create(customerId, items);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("UnitPrice must be greater than zero.");
    }

    [Fact]
    public void Cancel_WhenPending_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), [("Product A", 1, 10.0m)]);

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowDomainException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), [("Product A", 1, 10.0m)]);
        order.Cancel(); // Status is now Cancelled

        // Act
        Action act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Only orders with 'Pending' status can be cancelled.");
    }
}
