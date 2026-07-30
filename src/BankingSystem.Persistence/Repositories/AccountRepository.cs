using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;

    public AccountRepository(BankingDbContext context) => _context = context;

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Account?> GetByNumberAsync(string number, CancellationToken ct = default)
        => _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.Number == number, ct);

    public Task<List<Account>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Accounts.Include(a => a.User).Where(a => a.UserId == userId).ToListAsync(ct);

    public async Task AddAsync(Account account, CancellationToken ct = default)
        => await _context.Accounts.AddAsync(account, ct);

    public void Update(Account account) => _context.Accounts.Update(account);

    public Task<bool> NumberExistsAsync(string number, CancellationToken ct = default)
        => _context.Accounts.AnyAsync(a => a.Number == number, ct);
}
