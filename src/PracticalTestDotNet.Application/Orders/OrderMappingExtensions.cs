using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Domain.Entities;

namespace PracticalTestDotNet.Application.Orders;

public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        var items = order.Items.Select(item => new OrderItemDto(
            item.Id,
            item.OrderId,
            item.ProductName,
            item.Quantity,
            item.UnitPrice
        )).ToList();

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.CreatedAt,
            order.TotalAmount,
            items
        );
    }
}
