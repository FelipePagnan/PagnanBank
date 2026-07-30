using BankingSystem.Application.DTOs.Auth;
using BankingSystem.Application.Services.Auth;
using BankingSystem.Application.Validators.Auth;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Persistence.Repositories;
using BankingSystem.Tests.Common;
using FluentAssertions;
using Xunit;

namespace BankingSystem.Tests.Services;

public sealed class AuthServiceTests
{
    private static AuthService CreateSut(TestDatabase db, FakePasswordHasher hasher, FixedDateTimeProvider clock)
    {
        var users = new UserRepository(db.Context);
        var loginHistory = new LoginHistoryRepository(db.Context);
        var unitOfWork = new UnitOfWork(db.Context);
        return new AuthService(users, loginHistory, unitOfWork, hasher,
            new NullAuditService(), clock, new LoginRequestValidator());
    }

    private static async Task SeedUserAsync(TestDatabase db, FakePasswordHasher hasher, string email, string password)
    {
        db.Context.Users.Add(new User
        {
            FullName = "Teste",
            Email = email,
            Cpf = "12345678901",
            PasswordHash = hasher.Hash(password),
            Role = UserRole.Client,
            Status = UserStatus.Active
        });
        await db.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        using var db = new TestDatabase();
        var hasher = new FakePasswordHasher();
        var clock = new FixedDateTimeProvider();
        await SeedUserAsync(db, hasher, "user@bank.local", "Secret@123");

        var sut = CreateSut(db, hasher, clock);
        var result = await sut.LoginAsync(new LoginRequest { Email = "user@bank.local", Password = "Secret@123" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@bank.local");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        using var db = new TestDatabase();
        var hasher = new FakePasswordHasher();
        var clock = new FixedDateTimeProvider();
        await SeedUserAsync(db, hasher, "user@bank.local", "Secret@123");

        var sut = CreateSut(db, hasher, clock);
        var result = await sut.LoginAsync(new LoginRequest { Email = "user@bank.local", Password = "wrong" });

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_AfterMaxFailedAttempts_BlocksUser()
    {
        using var db = new TestDatabase();
        var hasher = new FakePasswordHasher();
        var clock = new FixedDateTimeProvider();
        await SeedUserAsync(db, hasher, "user@bank.local", "Secret@123");

        var sut = CreateSut(db, hasher, clock);

        for (var i = 0; i < AuthService.MaxFailedAttempts; i++)
            await sut.LoginAsync(new LoginRequest { Email = "user@bank.local", Password = "wrong" });

        var blocked = db.Context.Users.Single(u => u.Email == "user@bank.local");
        blocked.Status.Should().Be(UserStatus.Blocked);

        // Even the correct password should now be rejected.
        var afterBlock = await sut.LoginAsync(new LoginRequest { Email = "user@bank.local", Password = "Secret@123" });
        afterBlock.IsFailure.Should().BeTrue();
    }
}
