using PracticalTestDotNet.Domain.Enums;
using PracticalTestDotNet.Domain.Exceptions;

namespace PracticalTestDotNet.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(item => item.UnitPrice * item.Quantity);

    // EF Core constructor
    private Order() { }

    public static Order Create(Guid customerId, IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("CustomerId is required.");
        }

        var itemList = items?.ToList();
        if (itemList == null || itemList.Count == 0)
        {
            throw new DomainException("An order must contain at least one item.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (productName, quantity, unitPrice) in itemList)
        {
            var item = OrderItem.Create(order.Id, productName, quantity, unitPrice);
            order._items.Add(item);
        }

        return order;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Only orders with 'Pending' status can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }
}
