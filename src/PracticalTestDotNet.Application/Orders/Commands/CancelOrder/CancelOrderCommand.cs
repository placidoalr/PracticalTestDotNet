using FluentValidation;
using MediatR;
using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Domain.Exceptions;
using PracticalTestDotNet.Domain.Interfaces;

namespace PracticalTestDotNet.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest<OrderDto>;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
    }
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new DomainException($"Order with ID '{request.OrderId}' was not found.");
        }

        order.Cancel();

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return order.ToDto();
    }
}
