using FluentValidation;
using MediatR;
using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Domain.Interfaces;

namespace PracticalTestDotNet.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
    }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        return order?.ToDto();
    }
}
