using System.Windows.Input;
using BankingSystem.Application.DTOs.Users;
using BankingSystem.Application.Services.Users;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Desktop.ViewModels;

public sealed class RegisterViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IDialogService _dialog;

    private string _fullName = string.Empty;
    private string _cpf = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;

    public RegisterViewModel(IUserService userService, IDialogService dialog)
    {
        _userService = userService;
        _dialog = dialog;

        RegisterCommand = new AsyncRelayCommand(RegisterAsync, () => !IsBusy);
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(false));
    }

    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string Cpf { get => _cpf; set => SetProperty(ref _cpf, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

    /// <summary>Raised when the window should close. True = account created.</summary>
    public event Action<bool>? CloseRequested;

    public AsyncRelayCommand RegisterCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasError));

        if (string.IsNullOrWhiteSpace(Password) || Password != ConfirmPassword)
        {
            ErrorMessage = "As senhas não conferem.";
            OnPropertyChanged(nameof(HasError));
            return;
        }

        IsBusy = true;
        try
        {
            // Self-registration always creates a Client (never an administrator).
            var result = await _userService.RegisterClientAsync(new CreateUserRequest
            {
                FullName = FullName,
                Cpf = Cpf,
                Email = Email,
                Password = Password,
                Role = UserRole.Client,
                InitialBalance = 0m
            });

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                OnPropertyChanged(nameof(HasError));
                return;
            }

            _dialog.Info("Conta criada com sucesso! Use seu e-mail e senha para entrar.");
            CloseRequested?.Invoke(true);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
