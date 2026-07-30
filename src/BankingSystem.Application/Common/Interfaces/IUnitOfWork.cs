namespace BankingSystem.Application.Common.Interfaces;

/// <summary>
/// Commits all changes tracked across repositories in a single atomic operation.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
