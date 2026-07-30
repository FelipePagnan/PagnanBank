using System.Windows;
using System.Windows.Controls;
using BankingSystem.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BankingSystem.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = _viewModel;

        _viewModel.CloseRequested += OnCloseRequested;
        _viewModel.RegisterRequested += OnRegisterRequested;
    }

    private void OnCloseRequested(bool authenticated)
    {
        DialogResult = authenticated;
        Close();
    }

    private void OnRegisterRequested()
    {
        // Resolved from the same DI scope, so it shares the session's DbContext.
        var window = _serviceProvider.GetRequiredService<RegisterWindow>();
        window.Owner = this;

        var created = window.ShowDialog();
        if (created == true && window.DataContext is RegisterViewModel registerViewModel)
        {
            // Pre-fill the login with the freshly created e-mail for convenience.
            _viewModel.Email = registerViewModel.Email;
            PasswordBox.Clear();
        }
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
            _viewModel.Password = box.Password;
    }
}
