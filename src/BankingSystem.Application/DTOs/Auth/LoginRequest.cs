namespace BankingSystem.Application.DTOs.Auth;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Machine { get; set; } = string.Empty;
}
