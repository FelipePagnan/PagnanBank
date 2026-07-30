using BankingSystem.Domain.Common;

namespace BankingSystem.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CashbackPercent { get; set; }
    public bool IsActive { get; set; } = true;
}
