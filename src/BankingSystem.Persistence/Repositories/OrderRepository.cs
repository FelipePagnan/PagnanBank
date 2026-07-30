using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly BankingDbContext _context;

    public OrderRepository(BankingDbContext context) => _context = context;

    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await _context.Orders.AddAsync(order, ct);
}
