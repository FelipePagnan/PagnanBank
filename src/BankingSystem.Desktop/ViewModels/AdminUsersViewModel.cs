using System.Collections.ObjectModel;
using System.Windows.Input;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Users;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Transactions;
using BankingSystem.Application.Services.Users;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Desktop.ViewModels;

public sealed class AdminUsersViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IUserService _userService;
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly IDialogService _dialog;

    private UserDto? _selectedUser;
    private AccountDto? _selectedAccount;
    private decimal _adjustAmount;
    private string _adjustReason = string.Empty;

    // New-user form fields
    private string _newName = string.Empty;
    private string _newCpf = string.Empty;
    private string _newEmail = string.Empty;
    private string _newPassword = string.Empty;
    private decimal _newInitialBalance;
    private bool _newIsAdmin;

    public AdminUsersViewModel(
        IUserService userService,
        IAccountService accountService,
        ITransactionService transactionService,
        IDialogService dialog)
    {
        _userService = userService;
        _accountService = accountService;
        _transactionService = transactionService;
        _dialog = dialog;

        BlockCommand = new AsyncRelayCommand(BlockAsync, () => SelectedUser is not null);
        UnblockCommand = new AsyncRelayCommand(UnblockAsync, () => SelectedUser is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedUser is not null);
        CreateCommand = new AsyncRelayCommand(CreateAsync, () => !IsBusy);
        RefreshCommand = new AsyncRelayCommand(InitializeAsync, () => !IsBusy);
        CreditCommand = new AsyncRelayCommand(() => AdjustAsync(credit: true), () => _selectedAccount is not null);
        DebitCommand = new AsyncRelayCommand(() => AdjustAsync(credit: false), () => _selectedAccount is not null);
    }

    public ObservableCollection<UserDto> Users { get; } = new();

    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
                _ = LoadSelectedAccountAsync();
        }
    }

    // Selected user's account summary (for balance adjustments)
    public bool HasSelectedAccount => _selectedAccount is not null;
    public string SelectedAccountNumber => _selectedAccount?.Number ?? "-";
    public decimal SelectedAccountBalance => _selectedAccount?.Balance ?? 0m;

    public decimal AdjustAmount { get => _adjustAmount; set => SetProperty(ref _adjustAmount, value); }
    public string AdjustReason { get => _adjustReason; set => SetProperty(ref _adjustReason, value); }

    public string NewName { get => _newName; set => SetProperty(ref _newName, value); }
    public string NewCpf { get => _newCpf; set => SetProperty(ref _newCpf, value); }
    public string NewEmail { get => _newEmail; set => SetProperty(ref _newEmail, value); }
    public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
    public decimal NewInitialBalance { get => _newInitialBalance; set => SetProperty(ref _newInitialBalance, value); }
    public bool NewIsAdmin { get => _newIsAdmin; set => SetProperty(ref _newIsAdmin, value); }

    public ICommand BlockCommand { get; }
    public ICommand UnblockCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CreditCommand { get; }
    public ICommand DebitCommand { get; }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Users.Clear();
            var users = await _userService.GetAllAsync();
            foreach (var user in users)
                Users.Add(user);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSelectedAccountAsync()
    {
        _selectedAccount = null;

        if (_selectedUser is not null)
        {
            var accounts = await _accountService.GetByUserAsync(_selectedUser.Id);
            _selectedAccount = accounts.FirstOrDefault();
        }

        OnPropertyChanged(nameof(HasSelectedAccount));
        OnPropertyChanged(nameof(SelectedAccountNumber));
        OnPropertyChanged(nameof(SelectedAccountBalance));
    }

    private async Task BlockAsync()
    {
        if (SelectedUser is null) return;
        var result = await _userService.BlockAsync(SelectedUser.Id);
        if (result.IsFailure)
            _dialog.Error(result.Error.Message);
        await InitializeAsync();
    }

    private async Task UnblockAsync()
    {
        if (SelectedUser is null) return;
        var result = await _userService.UnblockAsync(SelectedUser.Id);
        if (result.IsFailure)
            _dialog.Error(result.Error.Message);
        await InitializeAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedUser is null) return;

        if (!_dialog.Confirm(
                $"Excluir definitivamente o usuário \"{SelectedUser.FullName}\"?\n\n" +
                "Esta ação remove a conta, o extrato e todos os produtos vinculados e não pode ser desfeita."))
            return;

        var result = await _userService.DeleteAsync(SelectedUser.Id);
        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        _dialog.Info("Usuário excluído com sucesso.");
        SelectedUser = null;
        await LoadSelectedAccountAsync();
        await InitializeAsync();
    }

    private async Task AdjustAsync(bool credit)
    {
        if (_selectedAccount is null)
        {
            _dialog.Error("O usuário selecionado não possui conta bancária.");
            return;
        }

        if (AdjustAmount <= 0)
        {
            _dialog.Error("Informe um valor maior que zero.");
            return;
        }

        var result = await _transactionService.AdminAdjustBalanceAsync(
            _selectedAccount.Id, AdjustAmount, credit,
            string.IsNullOrWhiteSpace(AdjustReason) ? "Ajuste administrativo" : AdjustReason);

        if (result.IsFailure)
        {
            _dialog.Error(result.Error.Message);
            return;
        }

        var verb = credit ? "creditado" : "debitado";
        _dialog.Info($"Saldo {verb} com sucesso.");

        AdjustAmount = 0;
        AdjustReason = string.Empty;
        await LoadSelectedAccountAsync();
    }

    private async Task CreateAsync()
    {
        IsBusy = true;
        try
        {
            var request = new CreateUserRequest
            {
                FullName = NewName,
                Cpf = NewCpf,
                Email = NewEmail,
                Password = NewPassword,
                Role = NewIsAdmin ? UserRole.Administrator : UserRole.Client,
                InitialBalance = NewInitialBalance
            };

            var result = await _userService.CreateAsync(request);
            if (result.IsFailure)
            {
                _dialog.Error(result.Error.Message);
                return;
            }

            _dialog.Info($"Usuário {result.Value.FullName} criado com sucesso.");

            NewName = NewCpf = NewEmail = NewPassword = string.Empty;
            NewInitialBalance = 0;
            NewIsAdmin = false;

            await InitializeAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
