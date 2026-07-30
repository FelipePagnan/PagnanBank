using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.DTOs.Auth;

public sealed class LoginResponse
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
}
