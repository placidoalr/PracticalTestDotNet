using FluentValidation;
using MediatR;
using PracticalTestDotNet.Application.Common;
using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Domain.Interfaces;

namespace PracticalTestDotNet.Application.Orders.Queries.GetOrdersPaginated;

public record GetOrdersPaginatedQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedList<OrderDto>>;

public class GetOrdersPaginatedQueryValidator : AbstractValidator<GetOrdersPaginatedQuery>
{
    public GetOrdersPaginatedQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public class GetOrdersPaginatedQueryHandler : IRequestHandler<GetOrdersPaginatedQuery, PaginatedList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersPaginatedQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PaginatedList<OrderDto>> Handle(GetOrdersPaginatedQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetPaginatedAsync(request.Page, request.PageSize, cancellationToken);
        var dtos = orders.Select(o => o.ToDto()).ToList();

        return new PaginatedList<OrderDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
