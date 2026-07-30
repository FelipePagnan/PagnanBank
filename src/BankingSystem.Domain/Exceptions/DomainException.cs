namespace BankingSystem.Domain.Exceptions;

/// <summary>
/// Thrown for truly exceptional/invariant violations inside the domain.
/// Expected business rule failures use the Result pattern instead.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
