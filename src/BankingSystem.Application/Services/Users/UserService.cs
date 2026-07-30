using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.Common.Mapping;
using BankingSystem.Application.DTOs.Users;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Common;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using FluentValidation;

namespace BankingSystem.Application.Services.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateUserRequest> _validator;

    public UserService(
        IUserRepository users,
        IAccountRepository accounts,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditService audit,
        IDateTimeProvider clock,
        ICurrentUser currentUser,
        IValidator<CreateUserRequest> validator)
    {
        _users = users;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _audit = audit;
        _clock = clock;
        _currentUser = currentUser;
        _validator = validator;
    }

    private bool IsAdmin => _currentUser.Role == UserRole.Administrator;

    public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (!IsAdmin)
            return Result.Failure<UserDto>(DomainErrors.Auth.Forbidden);

        return await CreateInternalAsync(request, request.Role, request.InitialBalance, ct);
    }

    public Task<Result<UserDto>> RegisterClientAsync(CreateUserRequest request, CancellationToken ct = default)
        // Self-registration: force Client role and zero initial balance (no admin required).
        => CreateInternalAsync(request, UserRole.Client, 0m, ct);

    private async Task<Result<UserDto>> CreateInternalAsync(CreateUserRequest request, UserRole role, decimal initialBalance, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<UserDto>(DomainErrors.Validation.Rule(message));
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var cpf = OnlyDigits(request.Cpf);

        if (await _users.EmailExistsAsync(email, ct))
            return Result.Failure<UserDto>(DomainErrors.Users.EmailInUse);

        if (await _users.CpfExistsAsync(cpf, ct))
            return Result.Failure<UserDto>(DomainErrors.Users.CpfInUse);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Cpf = cpf,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            Status = UserStatus.Active
        };

        var account = new Account
        {
            UserId = user.Id,
            Number = await GenerateAccountNumberAsync(ct),
            Branch = "0001",
            Type = AccountType.Checking,
            Status = AccountStatus.Active,
            Balance = initialBalance < 0 ? 0m : initialBalance
        };
        user.Accounts.Add(account);

        await _users.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("CreateUser", "Users", OperationResult.Success,
            $"Usuário criado: {user.Email} ({user.Role}).", user.Id, user.FullName, ct);

        return Result.Success(user.ToDto());
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (!IsAdmin)
            return new List<UserDto>();

        var users = await _users.GetAllAsync(ct);
        return users.Select(u => u.ToDto()).ToList();
    }

    public Task<Result> BlockAsync(Guid userId, CancellationToken ct = default)
        => SetStatusAsync(userId, UserStatus.Blocked, "BlockUser", ct);

    public Task<Result> UnblockAsync(Guid userId, CancellationToken ct = default)
        => SetStatusAsync(userId, UserStatus.Active, "UnblockUser", ct);

    public async Task<Result> DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        if (!IsAdmin)
            return Result.Failure(DomainErrors.Auth.Forbidden);

        if (_currentUser.UserId == userId)
            return Result.Failure(DomainErrors.Users.CannotDeleteSelf);

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure(DomainErrors.Users.NotFound);

        if (user.Role == UserRole.Administrator)
            return Result.Failure(DomainErrors.Users.CannotDeleteAdmin);

        // Cascades to accounts, transactions, cards, investments, loans and orders (FK ON DELETE CASCADE).
        _users.Remove(user);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("DeleteUser", "Users", OperationResult.Success,
            $"Usuário excluído: {user.Email}.", _currentUser.UserId, _currentUser.UserName, ct);

        return Result.Success();
    }

    private async Task<Result> SetStatusAsync(Guid userId, UserStatus status, string operation, CancellationToken ct)
    {
        if (!IsAdmin)
            return Result.Failure(DomainErrors.Auth.Forbidden);

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure(DomainErrors.Users.NotFound);

        user.Status = status;
        if (status == UserStatus.Active)
            user.FailedLoginAttempts = 0;

        user.UpdatedAtUtc = _clock.UtcNow;
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync(operation, "Users", OperationResult.Success,
            $"Status alterado para {status}.", user.Id, user.FullName, ct);

        return Result.Success();
    }

    private async Task<string> GenerateAccountNumberAsync(CancellationToken ct)
    {
        string number;
        do
        {
            number = Random.Shared.Next(10000, 99999) + "-" + Random.Shared.Next(0, 9);
        }
        while (await _accounts.NumberExistsAsync(number, ct));
        return number;
    }

    private static string OnlyDigits(string value)
        => new(value.Where(char.IsDigit).ToArray());
}
