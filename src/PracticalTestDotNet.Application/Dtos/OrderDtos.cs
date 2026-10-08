namespace PracticalTestDotNet.Application.Dtos;

public record OrderItemDto(
    Guid Id,
    Guid OrderId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    DateTime CreatedAt,
    decimal TotalAmount,
    IReadOnlyCollection<OrderItemDto> Items
);
