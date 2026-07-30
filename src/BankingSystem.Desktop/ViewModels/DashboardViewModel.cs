using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Investments;
using BankingSystem.Application.Services.Loans;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Session;

namespace BankingSystem.Desktop.ViewModels;

public sealed class DashboardViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IAccountService _accountService;
    private readonly IInvestmentService _investmentService;
    private readonly ILoanService _loanService;
    private readonly UserSession _session;

    private AccountDto? _account;
    private decimal _totalInvested;
    private decimal _loansOutstanding;
    private decimal _monthIncome;
    private decimal _monthExpense;

    public DashboardViewModel(
        IAccountService accountService,
        IInvestmentService investmentService,
        ILoanService loanService,
        UserSession session)
    {
        _accountService = accountService;
        _investmentService = investmentService;
        _loanService = loanService;
        _session = session;
    }

    public string OwnerName => _session.UserName;
    public string Branch => _account?.Branch ?? "-";
    public string Number => _account?.Number ?? "-";
    public decimal Balance => _account?.Balance ?? 0m;
    public decimal DailyLimit => _account?.DailyLimit ?? 0m;
    public bool HasAccount => _account is not null;

    public decimal TotalInvested { get => _totalInvested; private set => SetProperty(ref _totalInvested, value); }
    public decimal LoansOutstanding { get => _loansOutstanding; private set => SetProperty(ref _loansOutstanding, value); }
    public decimal MonthIncome { get => _monthIncome; private set => SetProperty(ref _monthIncome, value); }
    public decimal MonthExpense { get => _monthExpense; private set => SetProperty(ref _monthExpense, value); }

    public async Task InitializeAsync()
    {
        if (_session.UserId is null)
            return;

        IsBusy = true;
        try
        {
            var userId = _session.UserId.Value;
            var accounts = await _accountService.GetByUserAsync(userId);
            _account = accounts.FirstOrDefault();

            var investments = await _investmentService.GetByUserAsync(userId);
            TotalInvested = investments.Where(i => i.IsActive).Sum(i => i.EstimatedValue);

            var loans = await _loanService.GetByUserAsync(userId);
            LoansOutstanding = loans.Where(l => l.IsActive).Sum(l => l.Outstanding);

            if (_account is not null)
            {
                var statement = await _accountService.GetStatementAsync(_account.Id, 500);
                var now = DateTime.UtcNow;
                var monthly = statement.Where(t => t.TimestampUtc.Year == now.Year && t.TimestampUtc.Month == now.Month).ToList();
                MonthIncome = monthly.Where(t => t.IsCredit).Sum(t => t.Amount);
                MonthExpense = monthly.Where(t => !t.IsCredit).Sum(t => t.Amount);
            }

            OnPropertyChanged(nameof(Branch));
            OnPropertyChanged(nameof(Number));
            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(DailyLimit));
            OnPropertyChanged(nameof(HasAccount));
            OnPropertyChanged(nameof(OwnerName));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
