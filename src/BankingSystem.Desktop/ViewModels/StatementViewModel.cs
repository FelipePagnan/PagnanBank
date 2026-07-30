using System.Collections.ObjectModel;
using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Session;

namespace BankingSystem.Desktop.ViewModels;

public sealed class StatementViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IAccountService _accountService;
    private readonly UserSession _session;

    public StatementViewModel(IAccountService accountService, UserSession session)
    {
        _accountService = accountService;
        _session = session;
    }

    public ObservableCollection<TransactionDto> Transactions { get; } = new();

    public bool IsEmpty => Transactions.Count == 0;

    public async Task InitializeAsync()
    {
        if (_session.UserId is null)
            return;

        IsBusy = true;
        try
        {
            Transactions.Clear();

            var accounts = await _accountService.GetByUserAsync(_session.UserId.Value);
            var account = accounts.FirstOrDefault();
            if (account is null)
                return;

            var statement = await _accountService.GetStatementAsync(account.Id, 200);
            foreach (var transaction in statement)
                Transactions.Add(transaction);

            OnPropertyChanged(nameof(IsEmpty));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
