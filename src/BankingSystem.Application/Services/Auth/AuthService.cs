using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.DTOs.Auth;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Common;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using FluentValidation;

namespace BankingSystem.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    /// <summary>Number of consecutive failed attempts before the user is blocked.</summary>
    public const int MaxFailedAttempts = 5;

    private readonly IUserRepository _users;
    private readonly ILoginHistoryRepository _loginHistory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;
    private readonly IDateTimeProvider _clock;
    private readonly IValidator<LoginRequest> _validator;

    public AuthService(
        IUserRepository users,
        ILoginHistoryRepository loginHistory,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditService audit,
        IDateTimeProvider clock,
        IValidator<LoginRequest> validator)
    {
        _users = users;
        _loginHistory = loginHistory;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _audit = audit;
        _clock = clock;
        _validator = validator;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<LoginResponse>(DomainErrors.Validation.Rule(message));
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null)
        {
            await RecordLoginAsync(null, email, success: false, request.Machine, ct);
            await _audit.LogAsync("Login", "Auth", OperationResult.Failure,
                $"Tentativa com e-mail inexistente: {email}", ct: ct);
            return Result.Failure<LoginResponse>(DomainErrors.Auth.InvalidCredentials);
        }

        if (user.Status == UserStatus.Blocked)
        {
            await RecordLoginAsync(user.Id, email, success: false, request.Machine, ct);
            await _audit.LogAsync("Login", "Auth", OperationResult.Failure,
                "Login em usuário bloqueado.", user.Id, user.FullName, ct);
            return Result.Failure<LoginResponse>(DomainErrors.Auth.AccountLocked);
        }

        if (user.Status == UserStatus.Inactive)
        {
            await RecordLoginAsync(user.Id, email, success: false, request.Machine, ct);
            return Result.Failure<LoginResponse>(DomainErrors.Auth.UserInactive);
        }

        var passwordOk = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordOk)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
                user.Status = UserStatus.Blocked;

            user.UpdatedAtUtc = _clock.UtcNow;
            _users.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            await RecordLoginAsync(user.Id, email, success: false, request.Machine, ct);
            await _audit.LogAsync("Login", "Auth", OperationResult.Failure,
                $"Senha incorreta. Tentativas: {user.FailedLoginAttempts}.", user.Id, user.FullName, ct);

            return user.Status == UserStatus.Blocked
                ? Result.Failure<LoginResponse>(DomainErrors.Auth.AccountLocked)
                : Result.Failure<LoginResponse>(DomainErrors.Auth.InvalidCredentials);
        }

        // Success
        user.FailedLoginAttempts = 0;
        user.LastLoginAtUtc = _clock.UtcNow;
        user.UpdatedAtUtc = _clock.UtcNow;
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        await RecordLoginAsync(user.Id, email, success: true, request.Machine, ct);
        await _audit.LogAsync("Login", "Auth", OperationResult.Success,
            "Login realizado com sucesso.", user.Id, user.FullName, ct);

        return Result.Success(new LoginResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        });
    }

    private async Task RecordLoginAsync(Guid? userId, string email, bool success, string machine, CancellationToken ct)
    {
        await _loginHistory.AddAsync(new LoginHistory
        {
            UserId = userId,
            Email = email,
            Success = success,
            Machine = machine,
            TimestampUtc = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
