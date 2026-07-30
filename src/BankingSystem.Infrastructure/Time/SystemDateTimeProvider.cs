using BankingSystem.Application.Common.Interfaces;

namespace BankingSystem.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
