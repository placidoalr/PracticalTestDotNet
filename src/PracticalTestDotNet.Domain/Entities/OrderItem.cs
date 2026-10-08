using PracticalTestDotNet.Domain.Exceptions;

namespace PracticalTestDotNet.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // EF Core constructor
    private OrderItem() { }

    public static OrderItem Create(Guid orderId, string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Product name cannot be empty.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (unitPrice <= 0)
        {
            throw new DomainException("UnitPrice must be greater than zero.");
        }

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductName = productName.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
