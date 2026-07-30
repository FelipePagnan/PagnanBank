using BankingSystem.Application.DTOs.Cards;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Cards;

public interface ICardService
{
    Task<Result<CardDto>> IssueAsync(IssueCardRequest request, CancellationToken ct = default);
    Task<Result> BlockAsync(Guid cardId, CancellationToken ct = default);
    Task<Result> UnblockAsync(Guid cardId, CancellationToken ct = default);
    Task<Result> SetLimitAsync(Guid cardId, decimal newLimit, CancellationToken ct = default);
    Task<Result> PayInvoiceAsync(Guid cardId, CancellationToken ct = default);
    Task<List<CardDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
}
