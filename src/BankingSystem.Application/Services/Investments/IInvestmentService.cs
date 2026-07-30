using BankingSystem.Application.DTOs.Investments;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Investments;

public interface IInvestmentService
{
    Task<Result<InvestmentDto>> InvestAsync(CreateInvestmentRequest request, CancellationToken ct = default);
    Task<Result> RedeemAsync(Guid investmentId, CancellationToken ct = default);
    Task<List<InvestmentDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    InvestmentSimulationResult Simulate(InvestmentSimulationRequest request);
}
