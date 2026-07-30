namespace BankingSystem.Application.DTOs.Investments;

public sealed class CreateInvestmentRequest
{
    public Guid AccountId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Principal { get; set; }
    public decimal AnnualRatePercent { get; set; }
}
