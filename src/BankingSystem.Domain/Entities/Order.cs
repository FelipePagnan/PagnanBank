using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class Order : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public decimal Total { get; set; }
    public decimal CashbackAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public int Installments { get; set; } = 1;
    public Guid? CardId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Confirmed;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
