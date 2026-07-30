using System.Windows.Input;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Session;
using Microsoft.Extensions.DependencyInjection;

namespace BankingSystem.Desktop.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly IServiceProvider _provider;
    private readonly UserSession _session;
    private object? _currentView;

    public ShellViewModel(IServiceProvider provider, UserSession session)
    {
        _provider = provider;
        _session = session;

        ShowDashboardCommand = new RelayCommand(() => Navigate<DashboardViewModel>());
        ShowStatementCommand = new RelayCommand(() => Navigate<StatementViewModel>());
        ShowNewTransactionCommand = new RelayCommand(() => Navigate<NewTransactionViewModel>());
        ShowInvestmentsCommand = new RelayCommand(() => Navigate<InvestmentsViewModel>());
        ShowLoansCommand = new RelayCommand(() => Navigate<LoansViewModel>());
        ShowCardsCommand = new RelayCommand(() => Navigate<CardsViewModel>());
        ShowStoreCommand = new RelayCommand(() => Navigate<StoreViewModel>());
        ShowReportsCommand = new RelayCommand(() => Navigate<ReportsViewModel>());
        ShowSimulatorsCommand = new RelayCommand(() => Navigate<SimulatorsViewModel>());
        ShowAdminCommand = new RelayCommand(() => Navigate<AdminUsersViewModel>());
        ShowAuditCommand = new RelayCommand(() => Navigate<AuditViewModel>());
        ShowLoginHistoryCommand = new RelayCommand(() => Navigate<LoginHistoryViewModel>());
        LogoutCommand = new RelayCommand(Logout);

        Navigate<DashboardViewModel>();
    }

    /// <summary>Raised when the user chooses to log out; handled by the shell window.</summary>
    public event Action? LogoutRequested;

    public string CurrentUserName => _session.UserName;
    public bool IsAdministrator => _session.IsAdministrator;
    public string RoleLabel => IsAdministrator ? "Administrador" : "Cliente";

    public object? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ICommand ShowDashboardCommand { get; }
    public ICommand ShowStatementCommand { get; }
    public ICommand ShowNewTransactionCommand { get; }
    public ICommand ShowInvestmentsCommand { get; }
    public ICommand ShowLoansCommand { get; }
    public ICommand ShowCardsCommand { get; }
    public ICommand ShowStoreCommand { get; }
    public ICommand ShowReportsCommand { get; }
    public ICommand ShowSimulatorsCommand { get; }
    public ICommand ShowAdminCommand { get; }
    public ICommand ShowAuditCommand { get; }
    public ICommand ShowLoginHistoryCommand { get; }
    public ICommand LogoutCommand { get; }

    private void Logout()
    {
        _session.SignOut();
        LogoutRequested?.Invoke();
    }

    private void Navigate<TViewModel>() where TViewModel : class
    {
        var viewModel = _provider.GetRequiredService<TViewModel>();
        CurrentView = viewModel;

        if (viewModel is IAsyncInitializable initializable)
            _ = initializable.InitializeAsync();
    }
}
