namespace BankingSystem.Application.DTOs.Loans;

public sealed class LoanSimulationRequest
{
    public decimal Principal { get; set; }
    public decimal AnnualRatePercent { get; set; }
    public int Installments { get; set; }
}

public sealed class LoanSimulationResult
{
    public decimal Principal { get; init; }
    public decimal InstallmentAmount { get; init; }
    public decimal Total { get; init; }
    public decimal TotalInterest { get; init; }
}
