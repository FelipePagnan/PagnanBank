using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.Common.Interfaces;

/// <summary>
/// Represents the currently authenticated user for the running session.
/// Implemented by the presentation layer (UserSession).
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string UserName { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
