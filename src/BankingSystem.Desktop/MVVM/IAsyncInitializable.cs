namespace BankingSystem.Desktop.MVVM;

/// <summary>Implemented by page view models that need to load data when shown.</summary>
public interface IAsyncInitializable
{
    Task InitializeAsync();
}
