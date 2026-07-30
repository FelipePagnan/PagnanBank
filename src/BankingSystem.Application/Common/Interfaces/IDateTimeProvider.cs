namespace BankingSystem.Application.Common.Interfaces;

/// <summary>Abstraction over the system clock to keep services testable.</summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
