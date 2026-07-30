using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Application.Services.Auth;
using BankingSystem.Application.Services.Cards;
using BankingSystem.Application.Services.Investments;
using BankingSystem.Application.Services.Loans;
using BankingSystem.Application.Services.Security;
using BankingSystem.Application.Services.Store;
using BankingSystem.Application.Services.Transactions;
using BankingSystem.Application.Services.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BankingSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation: register every validator declared in this assembly.
        services.AddValidatorsFromAssemblyContaining<DependencyInjectionMarker>();

        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IInvestmentService, InvestmentService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ILoginHistoryService, LoginHistoryService>();

        return services;
    }
}

/// <summary>Anchor type used to locate this assembly for validator scanning.</summary>
public sealed class DependencyInjectionMarker;
