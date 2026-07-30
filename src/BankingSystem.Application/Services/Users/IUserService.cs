using BankingSystem.Application.DTOs.Users;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Users;

public interface IUserService
{
    /// <summary>Admin-only creation (can set the role).</summary>
    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

    /// <summary>Public self-registration; always creates a Client with zero balance.</summary>
    Task<Result<UserDto>> RegisterClientAsync(CreateUserRequest request, CancellationToken ct = default);

    Task<List<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result> BlockAsync(Guid userId, CancellationToken ct = default);
    Task<Result> UnblockAsync(Guid userId, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid userId, CancellationToken ct = default);
}
