using System.Collections.ObjectModel;
using BankingSystem.Application.DTOs.Audit;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Desktop.MVVM;

namespace BankingSystem.Desktop.ViewModels;

public sealed class AuditViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IAuditService _auditService;
    private readonly List<AuditLogDto> _all = new();

    private string _filterText = string.Empty;
    private string _selectedModule = "Todos";
    private string _selectedResult = "Todos";

    public AuditViewModel(IAuditService auditService)
    {
        _auditService = auditService;
        RefreshCommand = new AsyncRelayCommand(InitializeAsync, () => !IsBusy);
    }

    public ObservableCollection<AuditLogDto> Logs { get; } = new();
    public ObservableCollection<string> Modules { get; } = new() { "Todos" };
    public ObservableCollection<string> Results { get; } = new() { "Todos", "Sucesso", "Falha" };

    public string FilterText
    {
        get => _filterText;
        set { if (SetProperty(ref _filterText, value)) ApplyFilter(); }
    }

    public string SelectedModule
    {
        get => _selectedModule;
        set { if (SetProperty(ref _selectedModule, value)) ApplyFilter(); }
    }

    public string SelectedResult
    {
        get => _selectedResult;
        set { if (SetProperty(ref _selectedResult, value)) ApplyFilter(); }
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            _all.Clear();
            _all.AddRange(await _auditService.GetRecentAsync(500));

            var modules = _all.Select(l => l.Module).Distinct().OrderBy(m => m).ToList();
            Modules.Clear();
            Modules.Add("Todos");
            foreach (var module in modules)
                Modules.Add(module);
            _selectedModule = "Todos";
            OnPropertyChanged(nameof(SelectedModule));

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<AuditLogDto> query = _all;

        if (SelectedModule != "Todos")
            query = query.Where(l => l.Module == SelectedModule);

        if (SelectedResult != "Todos")
            query = query.Where(l => l.ResultLabel == SelectedResult);

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var term = FilterText.Trim().ToLowerInvariant();
            query = query.Where(l =>
                l.Operation.ToLowerInvariant().Contains(term) ||
                l.UserName.ToLowerInvariant().Contains(term) ||
                l.Details.ToLowerInvariant().Contains(term));
        }

        Logs.Clear();
        foreach (var log in query)
            Logs.Add(log);
    }
}
