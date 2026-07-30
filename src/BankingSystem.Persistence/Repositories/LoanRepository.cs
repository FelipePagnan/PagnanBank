using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class LoanRepository : ILoanRepository
{
    private readonly BankingDbContext _context;

    public LoanRepository(BankingDbContext context) => _context = context;

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Loans.Include(l => l.Account).FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<List<Loan>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Loans
            .Include(l => l.Account)
            .Where(l => l.Account!.UserId == userId)
            .OrderByDescending(l => l.ContractedAtUtc)
            .ToListAsync(ct);

    public async Task AddAsync(Loan loan, CancellationToken ct = default)
        => await _context.Loans.AddAsync(loan, ct);

    public void Update(Loan loan) => _context.Loans.Update(loan);
}
