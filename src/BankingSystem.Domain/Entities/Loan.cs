using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class Loan : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public decimal Principal { get; set; }
    public decimal AnnualRatePercent { get; set; }
    public int Installments { get; set; }
    public int PaidInstallments { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public DateTime ContractedAtUtc { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public decimal Outstanding => (Installments - PaidInstallments) * InstallmentAmount;
}
