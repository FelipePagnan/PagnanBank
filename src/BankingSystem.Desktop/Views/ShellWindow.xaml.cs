using System.Windows;
using BankingSystem.Desktop.ViewModels;

namespace BankingSystem.Desktop.Views;

public partial class ShellWindow : Window
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Logout closes the shell with a positive result so the App session loop
        // knows to return to the login window (instead of exiting the app).
        viewModel.LogoutRequested += OnLogoutRequested;
    }

    private void OnLogoutRequested()
    {
        DialogResult = true;
        Close();
    }
}
