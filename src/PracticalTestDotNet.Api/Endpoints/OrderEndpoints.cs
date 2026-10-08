using MediatR;
using Microsoft.AspNetCore.Mvc;
using PracticalTestDotNet.Application.Common;
using PracticalTestDotNet.Application.Dtos;
using PracticalTestDotNet.Application.Orders.Commands.CancelOrder;
using PracticalTestDotNet.Application.Orders.Commands.CreateOrder;
using PracticalTestDotNet.Application.Orders.Queries.GetOrderById;
using PracticalTestDotNet.Application.Orders.Queries.GetOrdersPaginated;

namespace PracticalTestDotNet.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateOrderCommand command,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/orders/{result.Id}", result);
        })
        .WithName("CreateOrder")
        .WithSummary("Cria um novo pedido com no mínimo 1 item (Requer Autenticação)")
        .Produces<OrderDto>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var p = page < 1 ? 1 : page;
            var ps = pageSize < 1 ? 10 : pageSize;

            var query = new GetOrdersPaginatedQuery(p, ps);
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetOrdersPaginated")
        .WithSummary("Lista pedidos com paginação (Requer Autenticação)")
        .Produces<PaginatedList<OrderDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrderByIdQuery(id);
            var result = await sender.Send(query, cancellationToken);

            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetOrderById")
        .WithSummary("Retorna o pedido por ID (Requer Autenticação)")
        .Produces<OrderDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/cancel", async (
            [FromRoute] Guid id,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CancelOrderCommand(id);
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CancelOrder")
        .WithSummary("Cancela um pedido existente em status Pending (Requer Autenticação)")
        .Produces<OrderDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
