using BankingSystem.Application.DTOs.Loans;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Loans;

public interface ILoanService
{
    Task<Result<LoanDto>> ContractAsync(CreateLoanRequest request, CancellationToken ct = default);
    Task<Result> PayInstallmentAsync(Guid loanId, CancellationToken ct = default);
    Task<List<LoanDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    LoanSimulationResult Simulate(LoanSimulationRequest request);
}
