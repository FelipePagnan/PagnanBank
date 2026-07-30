namespace BankingSystem.Application.DTOs.Investments;

public sealed class InvestmentSimulationRequest
{
    public decimal Principal { get; set; }
    public decimal AnnualRatePercent { get; set; }
    public int Months { get; set; }
}

public sealed class InvestmentSimulationResult
{
    public decimal Principal { get; init; }
    public decimal FutureValue { get; init; }
    public decimal Yield { get; init; }
}
