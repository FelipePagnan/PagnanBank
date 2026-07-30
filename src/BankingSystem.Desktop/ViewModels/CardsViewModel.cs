using System.Collections.ObjectModel;
using System.Windows.Input;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Cards;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Cards;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Desktop.Session;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Desktop.ViewModels;

public sealed class CardsViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ICardService _cardService;
    private readonly IAccountService _accountService;
    private readonly IDialogService _dialog;
    private readonly UserSession _session;

    private AccountDto? _account;
    private CardDto? _selected;
    private string _selectedType = "Virtual";
    private decimal _newLimit = 1000m;
    private decimal _limitChange;

    public CardsViewModel(
        ICardService cardService,
        IAccountService accountService,
        IDialogService dialog,
        UserSession session)
    {
        _cardService = cardService;
        _accountService = accountService;
        _dialog = dialog;
        _session = session;

        IssueCommand = new AsyncRelayCommand(IssueAsync, () => !IsBusy);
        BlockCommand = new AsyncRelayCommand(BlockAsync, () => Selected is not null);
        UnblockCommand = new AsyncRelayCommand(UnblockAsync, () => Selected is not null);
        SetLimitCommand = new AsyncRelayCommand(SetLimitAsync, () => Selected is not null);
        PayInvoiceCommand = new AsyncRelayCommand(PayInvoiceAsync, () => Selected is not null);
    }

    public ObservableCollection<CardDto> Cards { get; } = new();
    public ObservableCollection<string> Types { get; } = new() { "Virtual", "Físico" };

    public CardDto? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SelectedType { get => _selectedType; set => SetProperty(ref _selectedType, value); }
    public decimal NewLimit { get => _newLimit; set => SetProperty(ref _newLimit, value); }
    public decimal LimitChange { get => _limitChange; set => SetProperty(ref _limitChange, value); }

    public AsyncRelayCommand IssueCommand { get; }
    public AsyncRelayCommand BlockCommand { get; }
    public AsyncRelayCommand UnblockCommand { get; }
    public AsyncRelayCommand SetLimitCommand { get; }
    public AsyncRelayCommand PayInvoiceCommand { get; }

    public async Task InitializeAsync()
    {
        if (_session.UserId is null)
            return;

        IsBusy = true;
        try
        {
            var accounts = await _accountService.GetByUserAsync(_session.UserId.Value);
            _account = accounts.FirstOrDefault();
            await ReloadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadAsync()
    {
        if (_session.UserId is null) return;
        Cards.Clear();
        var list = await _cardService.GetByUserAsync(_session.UserId.Value);
        foreach (var card in list)
            Cards.Add(card);
    }

    private async Task IssueAsync()
    {
        if (_account is null)
        {
            _dialog.Error("Nenhuma conta encontrada para o usuário atual.");
            return;
        }
        if (NewLimit <= 0)
        {
            _dialog.Error("Informe um limite maior que zero.");
            return;
        }

        var result = await _cardService.IssueAsync(new IssueCardRequest
        {
            AccountId = _account.Id,
            Type = SelectedType == "Físico" ? CardType.Physical : CardType.Virtual,
            Limit = NewLimit
        });

        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        _dialog.Info("Cartão emitido com sucesso.");
        await ReloadAsync();
    }

    private async Task BlockAsync()
    {
        if (Selected is null) return;
        var result = await _cardService.BlockAsync(Selected.Id);
        if (result.IsFailure) _dialog.Error(result.Error.Message);
        await ReloadAsync();
    }

    private async Task UnblockAsync()
    {
        if (Selected is null) return;
        var result = await _cardService.UnblockAsync(Selected.Id);
        if (result.IsFailure) _dialog.Error(result.Error.Message);
        await ReloadAsync();
    }

    private async Task SetLimitAsync()
    {
        if (Selected is null) return;
        var result = await _cardService.SetLimitAsync(Selected.Id, LimitChange);
        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }
        _dialog.Info("Limite atualizado.");
        await ReloadAsync();
    }

    private async Task PayInvoiceAsync()
    {
        if (Selected is null) return;
        if (!_dialog.Confirm($"Pagar a fatura de {Selected.UsedAmount:C} do cartão {Selected.Number}?"))
            return;

        var result = await _cardService.PayInvoiceAsync(Selected.Id);
        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }
        _dialog.Info("Fatura paga com sucesso.");
        await ReloadAsync();
    }
}
