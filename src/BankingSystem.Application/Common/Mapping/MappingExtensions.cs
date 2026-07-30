using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Audit;
using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Application.DTOs.Users;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.Common.Mapping;

/// <summary>
/// Lightweight manual mapping between domain entities and DTOs.
/// Kept explicit (no AutoMapper) to make data flow obvious in a portfolio project.
/// </summary>
public static class MappingExtensions
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Cpf = user.Cpf,
        Email = user.Email,
        Role = user.Role,
        Status = user.Status,
        LastLoginAtUtc = user.LastLoginAtUtc
    };

    public static AccountDto ToDto(this Account account) => new()
    {
        Id = account.Id,
        Branch = account.Branch,
        Number = account.Number,
        Type = account.Type,
        Status = account.Status,
        Balance = account.Balance,
        DailyLimit = account.DailyLimit,
        OwnerName = account.User?.FullName ?? string.Empty
    };

    public static TransactionDto ToDto(this Transaction transaction) => new()
    {
        Id = transaction.Id,
        Type = transaction.Type,
        Amount = transaction.Amount,
        BalanceAfter = transaction.BalanceAfter,
        Description = transaction.Description,
        TimestampUtc = transaction.TimestampUtc
    };

    public static AuditLogDto ToDto(this AuditLog log) => new()
    {
        TimestampUtc = log.TimestampUtc,
        UserName = string.IsNullOrWhiteSpace(log.UserName) ? "-" : log.UserName,
        Operation = log.Operation,
        Module = log.Module,
        ResultLabel = log.Result == OperationResult.Success ? "Sucesso" : "Falha",
        Details = log.Details
    };
}
