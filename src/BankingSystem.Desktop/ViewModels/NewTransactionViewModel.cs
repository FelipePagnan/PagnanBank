using System.Collections.ObjectModel;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Transactions;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Desktop.Session;

namespace BankingSystem.Desktop.ViewModels;

public sealed class NewTransactionViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly IDialogService _dialog;
    private readonly UserSession _session;

    private AccountDto? _account;
    private string _selectedOperation = "Depósito";
    private decimal _amount;
    private string _destinationNumber = string.Empty;
    private string _description = string.Empty;

    public NewTransactionViewModel(
        ITransactionService transactionService,
        IAccountService accountService,
        IDialogService dialog,
        UserSession session)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _dialog = dialog;
        _session = session;

        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, () => !IsBusy);
    }

    public ObservableCollection<string> Operations { get; } = new()
    {
        "Depósito", "Saque", "Transferência", "PIX"
    };

    public string SelectedOperation
    {
        get => _selectedOperation;
        set
        {
            if (SetProperty(ref _selectedOperation, value))
                OnPropertyChanged(nameof(RequiresDestination));
        }
    }

    public bool RequiresDestination =>
        SelectedOperation is "Transferência" or "PIX";

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public string DestinationNumber
    {
        get => _destinationNumber;
        set => SetProperty(ref _destinationNumber, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string AccountNumber => _account?.Number ?? "-";
    public decimal Balance => _account?.Balance ?? 0m;

    public AsyncRelayCommand ExecuteCommand { get; }

    public async Task InitializeAsync()
    {
        if (_session.UserId is null)
            return;

        var accounts = await _accountService.GetByUserAsync(_session.UserId.Value);
        _account = accounts.FirstOrDefault();
        OnPropertyChanged(nameof(AccountNumber));
        OnPropertyChanged(nameof(Balance));
    }

    private async Task ExecuteAsync()
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

        IsBusy = true;
        try
        {
            var success = false;
            var errorMessage = string.Empty;

            switch (SelectedOperation)
            {
                case "Depósito":
                {
                    var result = await _transactionService.DepositAsync(new DepositRequest
                    {
                        AccountId = _account.Id,
                        Amount = Amount,
                        Description = string.IsNullOrWhiteSpace(Description) ? "Depósito" : Description
                    });
                    success = result.IsSuccess;
                    errorMessage = result.IsFailure ? result.Error.Message : string.Empty;
                    break;
                }
                case "Saque":
                {
                    var result = await _transactionService.WithdrawAsync(new WithdrawRequest
                    {
                        AccountId = _account.Id,
                        Amount = Amount,
                        Description = string.IsNullOrWhiteSpace(Description) ? "Saque" : Description
                    });
                    success = result.IsSuccess;
                    errorMessage = result.IsFailure ? result.Error.Message : string.Empty;
                    break;
                }
                default: // Transferência / PIX
                {
                    var result = await _transactionService.TransferAsync(new TransferRequest
                    {
                        SourceAccountId = _account.Id,
                        DestinationAccountNumber = DestinationNumber,
                        Amount = Amount,
                        Description = Description,
                        IsPix = SelectedOperation == "PIX"
                    });
                    success = result.IsSuccess;
                    errorMessage = result.IsFailure ? result.Error.Message : string.Empty;
                    break;
                }
            }

            if (!success)
            {
                _dialog.Error(errorMessage);
                return;
            }

            // Refresh balance and reset the form.
            await InitializeAsync();
            Amount = 0;
            DestinationNumber = string.Empty;
            Description = string.Empty;

            _dialog.Info($"{SelectedOperation} realizado com sucesso. Saldo atual: {Balance:C}.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
