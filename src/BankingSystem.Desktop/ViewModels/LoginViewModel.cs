using BankingSystem.Application.DTOs.Auth;
using BankingSystem.Application.Services.Auth;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Session;

namespace BankingSystem.Desktop.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly UserSession _session;

    private string _email = "admin@bank.local";
    private string _password = string.Empty;

    public LoginViewModel(IAuthService authService, UserSession session)
    {
        _authService = authService;
        _session = session;
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
        RegisterCommand = new RelayCommand(() => RegisterRequested?.Invoke());
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public AsyncRelayCommand LoginCommand { get; }
    public RelayCommand RegisterCommand { get; }

    /// <summary>Raised when the login window should close. True = authenticated.</summary>
    public event Action<bool>? CloseRequested;

    /// <summary>Raised when the user wants to open the self-registration window.</summary>
    public event Action? RegisterRequested;

    private async Task LoginAsync()
    {
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasError));
        IsBusy = true;

        try
        {
            var request = new LoginRequest
            {
                Email = Email,
                Password = Password,
                Machine = Environment.MachineName
            };

            var result = await _authService.LoginAsync(request);
            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                OnPropertyChanged(nameof(HasError));
                return;
            }

            _session.SignIn(result.Value);
            CloseRequested?.Invoke(true);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
