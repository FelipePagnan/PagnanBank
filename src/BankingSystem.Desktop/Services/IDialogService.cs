namespace BankingSystem.Desktop.Services;

public interface IDialogService
{
    void Info(string message, string title = "Informação");
    void Error(string message, string title = "Erro");
    bool Confirm(string message, string title = "Confirmação");
}
