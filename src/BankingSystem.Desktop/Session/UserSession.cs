using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.DTOs.Auth;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Desktop.Session;

/// <summary>
/// Holds the authenticated user for the lifetime of the application session.
/// Registered as a singleton and exposed to the Application layer via ICurrentUser.
/// </summary>
public sealed class UserSession : ICurrentUser
{
    public Guid? UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserRole? Role { get; private set; }

    public bool IsAuthenticated => UserId.HasValue;
    public bool IsAdministrator => Role == UserRole.Administrator;

    public void SignIn(LoginResponse response)
    {
        UserId = response.UserId;
        UserName = response.FullName;
        Email = response.Email;
        Role = response.Role;
    }

    public void SignOut()
    {
        UserId = null;
        UserName = string.Empty;
        Email = string.Empty;
        Role = null;
    }
}
