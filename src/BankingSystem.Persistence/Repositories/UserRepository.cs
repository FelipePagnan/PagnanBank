using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly BankingDbContext _context;

    public UserRepository(BankingDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _context.Users.Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByCpfAsync(string cpf, CancellationToken ct = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Cpf == cpf, ct);

    public Task<List<User>> GetAllAsync(CancellationToken ct = default)
        => _context.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public void Update(User user) => _context.Users.Update(user);

    public void Remove(User user) => _context.Users.Remove(user);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _context.Users.AnyAsync(u => u.Email == email, ct);

    public Task<bool> CpfExistsAsync(string cpf, CancellationToken ct = default)
        => _context.Users.AnyAsync(u => u.Cpf == cpf, ct);

    public Task<bool> AnyAsync(CancellationToken ct = default)
        => _context.Users.AnyAsync(ct);
}
