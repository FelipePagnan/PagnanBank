using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
}
