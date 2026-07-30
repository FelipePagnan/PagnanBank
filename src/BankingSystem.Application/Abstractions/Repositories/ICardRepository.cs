using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Card>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Card card, CancellationToken ct = default);
    void Update(Card card);
}
