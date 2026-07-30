using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.DTOs.Users;

public sealed class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Client;
    public decimal InitialBalance { get; set; }
}
