using FluentValidation;
using MediatR;
using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Domain.Entities;
using PracticalTestDotNet.Domain.Interfaces;

namespace PracticalTestDotNet.Application.Orders.Commands.CreateOrder;

public record CreateOrderItemDto(string ProductName, int Quantity, decimal UnitPrice);

public record CreateOrderCommand(Guid CustomerId, List<CreateOrderItemDto> Items) : IRequest<OrderDto>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductName)
                .NotEmpty().WithMessage("ProductName is required.");

            items.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            items.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice must be greater than zero.");
        });
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var itemTuples = request.Items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice));

        var order = Order.Create(request.CustomerId, itemTuples);

        await _orderRepository.AddAsync(order, cancellationToken);

        return order.ToDto();
    }
}
