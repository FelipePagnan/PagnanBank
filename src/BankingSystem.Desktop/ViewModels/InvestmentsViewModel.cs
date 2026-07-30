using System.Collections.ObjectModel;
using System.Windows.Input;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Investments;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Investments;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Desktop.Session;

namespace BankingSystem.Desktop.ViewModels;

public sealed class InvestmentsViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IInvestmentService _investmentService;
    private readonly IAccountService _accountService;
    private readonly IDialogService _dialog;
    private readonly UserSession _session;

    private AccountDto? _account;
    private InvestmentDto? _selected;

    private string _productName = "CDB Prefixado";
    private decimal _amount;
    private decimal _rate = 12m;

    private decimal _simAmount = 1000m;
    private decimal _simRate = 12m;
    private int _simMonths = 12;
    private string _simResult = string.Empty;

    public InvestmentsViewModel(
        IInvestmentService investmentService,
        IAccountService accountService,
        IDialogService dialog,
        UserSession session)
    {
        _investmentService = investmentService;
        _accountService = accountService;
        _dialog = dialog;
        _session = session;

        InvestCommand = new AsyncRelayCommand(InvestAsync, () => !IsBusy);
        RedeemCommand = new AsyncRelayCommand(RedeemAsync, () => Selected is not null && Selected.IsActive);
        SimulateCommand = new RelayCommand(Simulate);
    }

    public ObservableCollection<InvestmentDto> Investments { get; } = new();

    public InvestmentDto? Selected { get => _selected; set => SetProperty(ref _selected, value); }

    public string AccountNumber => _account?.Number ?? "-";
    public decimal Balance => _account?.Balance ?? 0m;

    public string ProductName { get => _productName; set => SetProperty(ref _productName, value); }
    public decimal Amount { get => _amount; set => SetProperty(ref _amount, value); }
    public decimal Rate { get => _rate; set => SetProperty(ref _rate, value); }

    public decimal SimAmount { get => _simAmount; set => SetProperty(ref _simAmount, value); }
    public decimal SimRate { get => _simRate; set => SetProperty(ref _simRate, value); }
    public int SimMonths { get => _simMonths; set => SetProperty(ref _simMonths, value); }
    public string SimResult { get => _simResult; private set => SetProperty(ref _simResult, value); }

    public AsyncRelayCommand InvestCommand { get; }
    public AsyncRelayCommand RedeemCommand { get; }
    public ICommand SimulateCommand { get; }

    public async Task InitializeAsync()
    {
        if (_session.UserId is null)
            return;

        IsBusy = true;
        try
        {
            var accounts = await _accountService.GetByUserAsync(_session.UserId.Value);
            _account = accounts.FirstOrDefault();
            OnPropertyChanged(nameof(AccountNumber));
            OnPropertyChanged(nameof(Balance));
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
        Investments.Clear();
        var list = await _investmentService.GetByUserAsync(_session.UserId.Value);
        foreach (var item in list)
            Investments.Add(item);
    }

    private async Task InvestAsync()
    {
        if (_account is null)
        {
            _dialog.Error("Nenhuma conta encontrada para o usuário atual.");
            return;
        }
        if (Amount <= 0)
        {
            _dialog.Error("Informe um valor maior que zero.");
            return;
        }

        var result = await _investmentService.InvestAsync(new CreateInvestmentRequest
        {
            AccountId = _account.Id,
            ProductName = ProductName,
            Principal = Amount,
            AnnualRatePercent = Rate
        });

        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        _dialog.Info($"Aplicação de {Amount:C} realizada com sucesso.");
        Amount = 0;
        await InitializeAsync();
    }

    private async Task RedeemAsync()
    {
        if (Selected is null) return;
        if (!_dialog.Confirm($"Resgatar o investimento \"{Selected.ProductName}\"?"))
            return;

        var result = await _investmentService.RedeemAsync(Selected.Id);
        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        _dialog.Info("Resgate realizado com sucesso.");
        await InitializeAsync();
    }

    private void Simulate()
    {
        var r = _investmentService.Simulate(new InvestmentSimulationRequest
        {
            Principal = SimAmount,
            AnnualRatePercent = SimRate,
            Months = SimMonths
        });
        SimResult = $"Valor final: {r.FutureValue:C}   |   Rendimento: {r.Yield:C}";
    }
}
