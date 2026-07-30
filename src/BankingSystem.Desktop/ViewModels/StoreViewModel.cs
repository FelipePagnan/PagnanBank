using System.Collections.ObjectModel;
using System.Windows.Input;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Cards;
using BankingSystem.Application.DTOs.Store;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Cards;
using BankingSystem.Application.Services.Store;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Desktop.Session;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Desktop.ViewModels;

public sealed class StoreViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IStoreService _storeService;
    private readonly ICardService _cardService;
    private readonly IAccountService _accountService;
    private readonly IDialogService _dialog;
    private readonly UserSession _session;

    private AccountDto? _account;
    private string _selectedPayment = "Débito (conta)";
    private int _installments = 1;
    private CardDto? _selectedCard;
    private decimal _total;
    private decimal _cashbackPreview;

    public StoreViewModel(
        IStoreService storeService,
        ICardService cardService,
        IAccountService accountService,
        IDialogService dialog,
        UserSession session)
    {
        _storeService = storeService;
        _cardService = cardService;
        _accountService = accountService;
        _dialog = dialog;
        _session = session;

        AddToCartCommand = new RelayCommand(p => AddToCart(p as ProductDto));
        IncreaseCommand = new RelayCommand(p => ChangeQuantity(p as CartLine, +1));
        DecreaseCommand = new RelayCommand(p => ChangeQuantity(p as CartLine, -1));
        RemoveCommand = new RelayCommand(p => RemoveFromCart(p as CartLine));
        CheckoutCommand = new AsyncRelayCommand(CheckoutAsync, () => Cart.Count > 0 && !IsBusy);
    }

    public ObservableCollection<ProductDto> Catalog { get; } = new();
    public ObservableCollection<CartLine> Cart { get; } = new();
    public ObservableCollection<CardDto> ActiveCards { get; } = new();
    public ObservableCollection<string> PaymentMethods { get; } = new() { "Débito (conta)", "Crédito (cartão)" };

    public decimal Balance => _account?.Balance ?? 0m;

    public string SelectedPayment
    {
        get => _selectedPayment;
        set
        {
            if (SetProperty(ref _selectedPayment, value))
                OnPropertyChanged(nameof(IsCredit));
        }
    }

    public bool IsCredit => SelectedPayment.StartsWith("Crédito");

    public int Installments { get => _installments; set => SetProperty(ref _installments, value); }
    public CardDto? SelectedCard { get => _selectedCard; set => SetProperty(ref _selectedCard, value); }

    public decimal Total { get => _total; private set => SetProperty(ref _total, value); }
    public decimal CashbackPreview { get => _cashbackPreview; private set => SetProperty(ref _cashbackPreview, value); }

    public ICommand AddToCartCommand { get; }
    public ICommand IncreaseCommand { get; }
    public ICommand DecreaseCommand { get; }
    public ICommand RemoveCommand { get; }
    public AsyncRelayCommand CheckoutCommand { get; }

    public async Task InitializeAsync()
    {
        if (_session.UserId is null)
            return;

        IsBusy = true;
        try
        {
            var accounts = await _accountService.GetByUserAsync(_session.UserId.Value);
            _account = accounts.FirstOrDefault();
            OnPropertyChanged(nameof(Balance));

            Catalog.Clear();
            foreach (var product in await _storeService.GetCatalogAsync())
                Catalog.Add(product);

            await ReloadCardsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadCardsAsync()
    {
        if (_session.UserId is null) return;
        ActiveCards.Clear();
        var cards = await _cardService.GetByUserAsync(_session.UserId.Value);
        foreach (var card in cards.Where(c => c.IsActive))
            ActiveCards.Add(card);
        SelectedCard = ActiveCards.FirstOrDefault();
    }

    private void AddToCart(ProductDto? product)
    {
        if (product is null) return;

        var existing = Cart.FirstOrDefault(l => l.ProductId == product.Id);
        if (existing is not null)
        {
            existing.Quantity++;
        }
        else
        {
            Cart.Add(new CartLine
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                CashbackPercent = product.CashbackPercent,
                Quantity = 1
            });
        }
        Recalculate();
    }

    private void ChangeQuantity(CartLine? line, int delta)
    {
        if (line is null) return;
        line.Quantity += delta;
        if (line.Quantity <= 0)
            Cart.Remove(line);
        Recalculate();
    }

    private void RemoveFromCart(CartLine? line)
    {
        if (line is null) return;
        Cart.Remove(line);
        Recalculate();
    }

    private void Recalculate()
    {
        Total = Cart.Sum(l => l.LineTotal);
        CashbackPreview = decimal.Round(Cart.Sum(l => l.LineTotal * l.CashbackPercent / 100m), 2);
    }

    private async Task CheckoutAsync()
    {
        if (_account is null)
        {
            _dialog.Error("Nenhuma conta encontrada para o usuário atual.");
            return;
        }
        if (Cart.Count == 0)
        {
            _dialog.Error("O carrinho está vazio.");
            return;
        }

        var isCredit = IsCredit;
        if (isCredit && SelectedCard is null)
        {
            _dialog.Error("Selecione um cartão para pagar no crédito.");
            return;
        }

        var request = new CheckoutRequest
        {
            AccountId = _account.Id,
            PaymentMethod = isCredit ? PaymentMethod.Credit : PaymentMethod.Debit,
            Installments = isCredit ? Math.Max(1, Installments) : 1,
            CardId = isCredit ? SelectedCard?.Id : null,
            Items = Cart.Select(l => new CheckoutItem { ProductId = l.ProductId, Quantity = l.Quantity }).ToList()
        };

        var result = await _storeService.CheckoutAsync(request);
        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        var summary = result.Value;
        _dialog.Info($"Compra confirmada!\nTotal: {summary.Total:C}\nPagamento: {summary.PaymentLabel}\nCashback: {summary.CashbackAmount:C}");

        Cart.Clear();
        Recalculate();
        await InitializeAsync();
    }
}
