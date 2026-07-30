using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class CardRepository : ICardRepository
{
    private readonly BankingDbContext _context;

    public CardRepository(BankingDbContext context) => _context = context;

    public Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Cards.Include(c => c.Account).ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Card>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Cards
            .Include(c => c.Account)
            .Where(c => c.Account!.UserId == userId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task AddAsync(Card card, CancellationToken ct = default)
        => await _context.Cards.AddAsync(card, ct);

    public void Update(Card card) => _context.Cards.Update(card);
}
