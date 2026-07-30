namespace BankingSystem.Application.DTOs.Investments;

public sealed class InvestmentDto
{
    public Guid Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Principal { get; init; }
    public decimal AnnualRatePercent { get; init; }
    public DateTime StartDateUtc { get; init; }
    public decimal EstimatedValue { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
