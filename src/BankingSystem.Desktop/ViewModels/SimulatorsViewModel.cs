using System.Globalization;
using System.Windows.Input;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Domain.Common;

namespace BankingSystem.Desktop.ViewModels;

public sealed class SimulatorsViewModel : ViewModelBase
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pt-BR");
    private static string Money(decimal v) => v.ToString("C", Culture);

    // ---- Financiamento (SAC) ----
    private decimal _finPrincipal = 60000m;
    private decimal _finRate = 12m;
    private int _finMonths = 60;
    private string _finResult = string.Empty;

    // ---- Quitação antecipada ----
    private decimal _qaInstallment = 500m;
    private decimal _qaRate = 24m;
    private int _qaRemaining = 10;
    private string _qaResult = string.Empty;

    // ---- Renegociação ----
    private decimal _rnOutstanding = 8000m;
    private decimal _rnRate = 18m;
    private int _rnMonths = 24;
    private string _rnResult = string.Empty;

    public SimulatorsViewModel()
    {
        SimulateFinancingCommand = new RelayCommand(SimulateFinancing);
        SimulateEarlyPayoffCommand = new RelayCommand(SimulateEarlyPayoff);
        SimulateRenegotiationCommand = new RelayCommand(SimulateRenegotiation);
    }

    public decimal FinPrincipal { get => _finPrincipal; set => SetProperty(ref _finPrincipal, value); }
    public decimal FinRate { get => _finRate; set => SetProperty(ref _finRate, value); }
    public int FinMonths { get => _finMonths; set => SetProperty(ref _finMonths, value); }
    public string FinResult { get => _finResult; private set => SetProperty(ref _finResult, value); }

    public decimal QaInstallment { get => _qaInstallment; set => SetProperty(ref _qaInstallment, value); }
    public decimal QaRate { get => _qaRate; set => SetProperty(ref _qaRate, value); }
    public int QaRemaining { get => _qaRemaining; set => SetProperty(ref _qaRemaining, value); }
    public string QaResult { get => _qaResult; private set => SetProperty(ref _qaResult, value); }

    public decimal RnOutstanding { get => _rnOutstanding; set => SetProperty(ref _rnOutstanding, value); }
    public decimal RnRate { get => _rnRate; set => SetProperty(ref _rnRate, value); }
    public int RnMonths { get => _rnMonths; set => SetProperty(ref _rnMonths, value); }
    public string RnResult { get => _rnResult; private set => SetProperty(ref _rnResult, value); }

    public ICommand SimulateFinancingCommand { get; }
    public ICommand SimulateEarlyPayoffCommand { get; }
    public ICommand SimulateRenegotiationCommand { get; }

    private void SimulateFinancing()
    {
        var first = FinancialCalculator.SacFirstInstallment(FinPrincipal, FinRate, FinMonths);
        var last = FinancialCalculator.SacLastInstallment(FinPrincipal, FinRate, FinMonths);
        var total = FinancialCalculator.SacTotal(FinPrincipal, FinRate, FinMonths);
        var interest = decimal.Round(total - FinPrincipal, 2);
        FinResult =
            $"Sistema SAC (parcelas decrescentes)\n" +
            $"1ª parcela: {Money(first)}   |   última parcela: {Money(last)}\n" +
            $"Total pago: {Money(total)}   |   juros: {Money(interest)}";
    }

    private void SimulateEarlyPayoff()
    {
        var nominal = decimal.Round(QaInstallment * QaRemaining, 2);
        var payoff = FinancialCalculator.PresentValueOfInstallments(QaInstallment, QaRate, QaRemaining);
        var savings = decimal.Round(nominal - payoff, 2);
        QaResult =
            $"Valor nominal restante: {Money(nominal)}\n" +
            $"Valor para quitar hoje: {Money(payoff)}\n" +
            $"Economia estimada: {Money(savings)}";
    }

    private void SimulateRenegotiation()
    {
        var installment = FinancialCalculator.MonthlyInstallment(RnOutstanding, RnRate, RnMonths);
        var total = FinancialCalculator.LoanTotal(RnOutstanding, RnRate, RnMonths);
        var interest = decimal.Round(total - RnOutstanding, 2);
        RnResult =
            $"Nova parcela: {Money(installment)} em {RnMonths}x\n" +
            $"Novo total: {Money(total)}   |   juros: {Money(interest)}";
    }
}
