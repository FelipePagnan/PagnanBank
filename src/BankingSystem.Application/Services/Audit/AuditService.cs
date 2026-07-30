using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.Common.Mapping;
using BankingSystem.Application.DTOs.Audit;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.Services.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public AuditService(IAuditLogRepository auditLogs, IUnitOfWork unitOfWork, IDateTimeProvider clock)
    {
        _auditLogs = auditLogs;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task LogAsync(
        string operation,
        string module,
        OperationResult result,
        string details = "",
        Guid? userId = null,
        string userName = "",
        CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Operation = operation,
            Module = module,
            Result = result,
            Details = details,
            UserId = userId,
            UserName = userName,
            TimestampUtc = _clock.UtcNow
        };

        await _auditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<List<AuditLogDto>> GetRecentAsync(int take = 200, CancellationToken ct = default)
    {
        var logs = await _auditLogs.GetRecentAsync(take, ct);
        return logs.Select(l => l.ToDto()).ToList();
    }
}
