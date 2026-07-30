namespace BankingSystem.Application.DTOs.Loans;

public sealed class LoanDto
{
    public Guid Id { get; init; }
    public decimal Principal { get; init; }
    public decimal AnnualRatePercent { get; init; }
    public int Installments { get; init; }
    public int PaidInstallments { get; init; }
    public decimal InstallmentAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal Outstanding { get; init; }
    public DateTime ContractedAtUtc { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
