using System.Windows;
using System.Windows.Controls;
using BankingSystem.Desktop.ViewModels;

namespace BankingSystem.Desktop.Views;

public partial class RegisterWindow : Window
{
    private readonly RegisterViewModel _viewModel;

    public RegisterWindow(RegisterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested(bool created)
    {
        DialogResult = created;
        Close();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
            _viewModel.Password = box.Password;
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
            _viewModel.ConfirmPassword = box.Password;
    }
}
