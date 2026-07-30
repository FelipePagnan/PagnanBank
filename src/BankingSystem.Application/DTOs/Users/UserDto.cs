using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.DTOs.Users;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public UserStatus Status { get; init; }
    public DateTime? LastLoginAtUtc { get; init; }
}
