using BankingSystem.Application.DTOs.Auth;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
