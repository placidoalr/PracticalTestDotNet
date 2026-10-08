using Microsoft.EntityFrameworkCore;
using PracticalTestDotNet.Domain.Entities;
using PracticalTestDotNet.Domain.Interfaces;
using PracticalTestDotNet.Infrastructure.Persistence;

namespace PracticalTestDotNet.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
