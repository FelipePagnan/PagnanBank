using BankingSystem.Desktop.MVVM;

namespace BankingSystem.Desktop.ViewModels;

/// <summary>A single line in the shopping cart (UI-only state).</summary>
public sealed class CartLine : ObservableObject
{
    private int _quantity;

    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public decimal CashbackPercent { get; init; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
                OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal LineTotal => UnitPrice * Quantity;
}
