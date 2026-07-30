using System.Collections.ObjectModel;
using System.Windows.Input;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Loans;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Loans;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Desktop.Session;

namespace BankingSystem.Desktop.ViewModels;

public sealed class LoansViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ILoanService _loanService;
    private readonly IAccountService _accountService;
    private readonly IDialogService _dialog;
    private readonly UserSession _session;

    private AccountDto? _account;
    private LoanDto? _selected;

    private decimal _amount;
    private decimal _rate = 24m;
    private int _installments = 12;

    private decimal _simAmount = 5000m;
    private decimal _simRate = 24m;
    private int _simInstallments = 12;
    private string _simResult = string.Empty;

    public LoansViewModel(
        ILoanService loanService,
        IAccountService accountService,
        IDialogService dialog,
        UserSession session)
    {
        _loanService = loanService;
        _accountService = accountService;
        _dialog = dialog;
        _session = session;

        ContractCommand = new AsyncRelayCommand(ContractAsync, () => !IsBusy);
        PayCommand = new AsyncRelayCommand(PayAsync, () => Selected is not null && Selected.IsActive);
        SimulateCommand = new RelayCommand(Simulate);
    }

    public ObservableCollection<LoanDto> Loans { get; } = new();

    public LoanDto? Selected { get => _selected; set => SetProperty(ref _selected, value); }

    public string AccountNumber => _account?.Number ?? "-";
    public decimal Balance => _account?.Balance ?? 0m;

    public decimal Amount { get => _amount; set => SetProperty(ref _amount, value); }
    public decimal Rate { get => _rate; set => SetProperty(ref _rate, value); }
    public int Installments { get => _installments; set => SetProperty(ref _installments, value); }

    public decimal SimAmount { get => _simAmount; set => SetProperty(ref _simAmount, value); }
    public decimal SimRate { get => _simRate; set => SetProperty(ref _simRate, value); }
    public int SimInstallments { get => _simInstallments; set => SetProperty(ref _simInstallments, value); }
    public string SimResult { get => _simResult; private set => SetProperty(ref _simResult, value); }

    public AsyncRelayCommand ContractCommand { get; }
    public AsyncRelayCommand PayCommand { get; }
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
        Loans.Clear();
        var list = await _loanService.GetByUserAsync(_session.UserId.Value);
        foreach (var item in list)
            Loans.Add(item);
    }

    private async Task ContractAsync()
    {
        if (_account is null)
        {
            _dialog.Error("Nenhuma conta encontrada para o usuário atual.");
            return;
        }
        if (Amount <= 0 || Installments <= 0)
        {
            _dialog.Error("Informe valor e número de parcelas válidos.");
            return;
        }

        var result = await _loanService.ContractAsync(new CreateLoanRequest
        {
            AccountId = _account.Id,
            Principal = Amount,
            AnnualRatePercent = Rate,
            Installments = Installments
        });

        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        _dialog.Info($"Empréstimo de {Amount:C} contratado. Parcela: {result.Value.InstallmentAmount:C}.");
        Amount = 0;
        await InitializeAsync();
    }

    private async Task PayAsync()
    {
        if (Selected is null) return;
        if (!_dialog.Confirm($"Pagar uma parcela de {Selected.InstallmentAmount:C}?"))
            return;

        var result = await _loanService.PayInstallmentAsync(Selected.Id);
        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        _dialog.Info("Parcela paga com sucesso.");
        await InitializeAsync();
    }

    private void Simulate()
    {
        var r = _loanService.Simulate(new LoanSimulationRequest
        {
            Principal = SimAmount,
            AnnualRatePercent = SimRate,
            Installments = SimInstallments
        });
        SimResult = $"Parcela: {r.InstallmentAmount:C}   |   Total: {r.Total:C}   |   Juros: {r.TotalInterest:C}";
    }
}
