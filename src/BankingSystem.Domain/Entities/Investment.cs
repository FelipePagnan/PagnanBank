using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class Investment : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal Principal { get; set; }
    public decimal AnnualRatePercent { get; set; }

    public DateTime StartDateUtc { get; set; }
    public DateTime? RedeemedAtUtc { get; set; }
    public decimal? RedeemedAmount { get; set; }

    public InvestmentStatus Status { get; set; } = InvestmentStatus.Active;
}
