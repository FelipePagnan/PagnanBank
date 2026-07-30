using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Application.Services.Transactions;
using BankingSystem.Application.Validators.Transactions;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Persistence.Repositories;
using BankingSystem.Tests.Common;
using FluentAssertions;
using Xunit;

namespace BankingSystem.Tests.Services;

public sealed class TransactionServiceTests
{
    private static TransactionService CreateSut(TestDatabase db, FixedDateTimeProvider clock)
    {
        var accounts = new AccountRepository(db.Context);
        var transactions = new TransactionRepository(db.Context);
        var unitOfWork = new UnitOfWork(db.Context);
        return new TransactionService(accounts, transactions, unitOfWork,
            new NullAuditService(), clock, new TestCurrentUser(),
            new DepositRequestValidator(), new WithdrawRequestValidator(), new TransferRequestValidator());
    }

    private static async Task<Account> SeedAccountAsync(TestDatabase db, string number, decimal balance, AccountStatus status = AccountStatus.Active)
    {
        var user = new User
        {
            FullName = "Owner " + number,
            Email = $"owner{number}@bank.local",
            Cpf = Guid.NewGuid().ToString("N")[..11],
            PasswordHash = "x",
            Role = UserRole.Client,
            Status = UserStatus.Active
        };
        var account = new Account
        {
            UserId = user.Id,
            Number = number,
            Branch = "0001",
            Type = AccountType.Checking,
            Status = status,
            Balance = balance
        };
        user.Accounts.Add(account);
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();
        return account;
    }

    [Fact]
    public async Task WithdrawAsync_WithInsufficientFunds_Fails()
    {
        using var db = new TestDatabase();
        var clock = new FixedDateTimeProvider();
        var account = await SeedAccountAsync(db, "10001-0", 100m);

        var sut = CreateSut(db, clock);
        var result = await sut.WithdrawAsync(new WithdrawRequest { AccountId = account.Id, Amount = 500m });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Account.InsufficientFunds");
    }

    [Fact]
    public async Task WithdrawAsync_OnBlockedAccount_Fails()
    {
        using var db = new TestDatabase();
        var clock = new FixedDateTimeProvider();
        var account = await SeedAccountAsync(db, "10002-0", 1000m, AccountStatus.Blocked);

        var sut = CreateSut(db, clock);
        var result = await sut.WithdrawAsync(new WithdrawRequest { AccountId = account.Id, Amount = 100m });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Account.Blocked");
    }

    [Fact]
    public async Task TransferAsync_MovesFundsBetweenAccounts()
    {
        using var db = new TestDatabase();
        var clock = new FixedDateTimeProvider();
        var source = await SeedAccountAsync(db, "10001-0", 1000m);
        var destination = await SeedAccountAsync(db, "20002-0", 200m);

        var sut = CreateSut(db, clock);
        var result = await sut.TransferAsync(new TransferRequest
        {
            SourceAccountId = source.Id,
            DestinationAccountNumber = "20002-0",
            Amount = 300m
        });

        result.IsSuccess.Should().BeTrue();

        var updatedSource = db.Context.Accounts.Single(a => a.Id == source.Id);
        var updatedDestination = db.Context.Accounts.Single(a => a.Id == destination.Id);

        updatedSource.Balance.Should().Be(700m);
        updatedDestination.Balance.Should().Be(500m);
        db.Context.Transactions.Count().Should().Be(2);
    }
}
