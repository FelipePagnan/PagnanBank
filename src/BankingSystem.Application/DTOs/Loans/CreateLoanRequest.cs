namespace BankingSystem.Application.DTOs.Loans;

public sealed class CreateLoanRequest
{
    public Guid AccountId { get; set; }
    public decimal Principal { get; set; }
    public decimal AnnualRatePercent { get; set; }
    public int Installments { get; set; }
}
