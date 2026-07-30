using System.Collections.ObjectModel;
using BankingSystem.Application.DTOs.Security;
using BankingSystem.Application.Services.Security;
using BankingSystem.Desktop.MVVM;

namespace BankingSystem.Desktop.ViewModels;

public sealed class LoginHistoryViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ILoginHistoryService _loginHistoryService;

    public LoginHistoryViewModel(ILoginHistoryService loginHistoryService)
    {
        _loginHistoryService = loginHistoryService;
        RefreshCommand = new AsyncRelayCommand(InitializeAsync, () => !IsBusy);
    }

    public ObservableCollection<LoginHistoryDto> Entries { get; } = new();

    public AsyncRelayCommand RefreshCommand { get; }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Entries.Clear();
            var entries = await _loginHistoryService.GetRecentAsync(300);
            foreach (var entry in entries)
                Entries.Add(entry);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
