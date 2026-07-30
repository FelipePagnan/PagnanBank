using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Infrastructure.Security;
using BankingSystem.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace BankingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        return services;
    }
}
