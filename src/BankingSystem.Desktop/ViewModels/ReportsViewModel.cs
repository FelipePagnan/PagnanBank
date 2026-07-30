using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Input;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Application.Services.Accounts;
using BankingSystem.Application.Services.Investments;
using BankingSystem.Application.Services.Loans;
using BankingSystem.Desktop.MVVM;
using BankingSystem.Desktop.Services;
using BankingSystem.Desktop.Session;
using BankingSystem.Domain.Enums;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankingSystem.Desktop.ViewModels;

public sealed class ReportsViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IAccountService _accountService;
    private readonly IInvestmentService _investmentService;
    private readonly ILoanService _loanService;
    private readonly IDialogService _dialog;
    private readonly UserSession _session;

    private AccountDto? _account;
    private List<TransactionDto> _statement = new();

    private decimal _totalInvested;
    private decimal _loansOutstanding;
    private decimal _monthIncome;
    private decimal _monthExpense;
    private decimal _totalCashback;
    private int _transactionCount;

    public ReportsViewModel(
        IAccountService accountService,
        IInvestmentService investmentService,
        ILoanService loanService,
        IDialogService dialog,
        UserSession session)
    {
        _accountService = accountService;
        _investmentService = investmentService;
        _loanService = loanService;
        _dialog = dialog;
        _session = session;

        RefreshCommand = new AsyncRelayCommand(InitializeAsync, () => !IsBusy);
        ExportStatementCsvCommand = new RelayCommand(ExportStatementCsv, () => _account is not null);
        ExportReportHtmlCommand = new RelayCommand(ExportReportHtml, () => _account is not null);
        ExportXlsxCommand = new RelayCommand(ExportXlsx, () => _account is not null);
        ExportPdfCommand = new RelayCommand(ExportPdf, () => _account is not null);
    }

    public string OwnerName => _session.UserName;
    public bool HasAccount => _account is not null;
    public string AccountNumber => _account?.Number ?? "-";
    public decimal Balance => _account?.Balance ?? 0m;
    public decimal TotalInvested { get => _totalInvested; private set => SetProperty(ref _totalInvested, value); }
    public decimal LoansOutstanding { get => _loansOutstanding; private set => SetProperty(ref _loansOutstanding, value); }
    public decimal MonthIncome { get => _monthIncome; private set => SetProperty(ref _monthIncome, value); }
    public decimal MonthExpense { get => _monthExpense; private set => SetProperty(ref _monthExpense, value); }
    public decimal TotalCashback { get => _totalCashback; private set => SetProperty(ref _totalCashback, value); }
    public int TransactionCount { get => _transactionCount; private set => SetProperty(ref _transactionCount, value); }

    public AsyncRelayCommand RefreshCommand { get; }
    public ICommand ExportStatementCsvCommand { get; }
    public ICommand ExportReportHtmlCommand { get; }
    public ICommand ExportXlsxCommand { get; }
    public ICommand ExportPdfCommand { get; }

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

            _statement = _account is null
                ? new List<TransactionDto>()
                : await _accountService.GetStatementAsync(_account.Id, 1000);

            TransactionCount = _statement.Count;
            TotalCashback = _statement
                .Where(t => t.Type == TransactionType.Cashback)
                .Sum(t => t.Amount);

            var now = DateTime.UtcNow;
            var monthly = _statement.Where(t => t.TimestampUtc.Year == now.Year && t.TimestampUtc.Month == now.Month).ToList();
            MonthIncome = monthly.Where(t => t.IsCredit).Sum(t => t.Amount);
            MonthExpense = monthly.Where(t => !t.IsCredit).Sum(t => t.Amount);

            OnPropertyChanged(nameof(HasAccount));
            OnPropertyChanged(nameof(AccountNumber));
            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(OwnerName));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ExportStatementCsv()
    {
        if (_account is null) return;

        var dialog = new SaveFileDialog
        {
            Title = "Exportar extrato (CSV)",
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = $"extrato_{_account.Number}_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dialog.ShowDialog() != true)
            return;

        var culture = CultureInfo.GetCultureInfo("pt-BR");
        string N(decimal v) => v.ToString("N2", culture);

        var totalCredits = _statement.Where(t => t.IsCredit).Sum(t => t.Amount);
        var totalDebits = _statement.Where(t => !t.IsCredit).Sum(t => t.Amount);

        var sb = new StringBuilder();

        // --- Cabeçalho / metadados ---
        sb.AppendLine("PagnanBank - Relatório Financeiro");
        sb.AppendLine($"Titular;{Escape(OwnerName)}");
        sb.AppendLine($"Conta;{Escape(AccountNumber)}");
        sb.AppendLine($"Gerado em;{DateTime.Now.ToString("dd/MM/yyyy HH:mm", culture)}");
        sb.AppendLine();

        // --- Resumo ---
        sb.AppendLine("Resumo");
        sb.AppendLine($"Saldo atual;{N(Balance)}");
        sb.AppendLine($"Total investido;{N(TotalInvested)}");
        sb.AppendLine($"Empréstimos em aberto;{N(LoansOutstanding)}");
        sb.AppendLine($"Entradas (mês);{N(MonthIncome)}");
        sb.AppendLine($"Saídas (mês);{N(MonthExpense)}");
        sb.AppendLine($"Cashback acumulado;{N(TotalCashback)}");
        sb.AppendLine($"Movimentações;{TransactionCount}");
        sb.AppendLine();

        // --- Extrato detalhado ---
        sb.AppendLine("Extrato");
        sb.AppendLine("Data;Hora;Operação;Descrição;Tipo;Entrada (R$);Saída (R$);Saldo após (R$)");
        foreach (var t in _statement)
        {
            var entrada = t.IsCredit ? N(t.Amount) : string.Empty;
            var saida = t.IsCredit ? string.Empty : N(t.Amount);
            sb.AppendLine(string.Join(";",
                t.TimestampUtc.ToString("dd/MM/yyyy", culture),
                t.TimestampUtc.ToString("HH:mm", culture),
                Escape(t.TypeLabel),
                Escape(t.Description),
                t.IsCredit ? "Crédito" : "Débito",
                entrada,
                saida,
                N(t.BalanceAfter)));
        }

        // --- Totais ---
        sb.AppendLine();
        sb.AppendLine($"Totais;;;;;{N(totalCredits)};{N(totalDebits)};");
        sb.AppendLine($"Resultado (entradas - saídas);;;;;;;{N(totalCredits - totalDebits)}");

        try
        {
            File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));
            _dialog.Info("Extrato exportado com sucesso.");
        }
        catch (Exception ex)
        {
            _dialog.Error("Não foi possível salvar o arquivo: " + ex.Message);
        }
    }

    private void ExportReportHtml()
    {
        if (_account is null) return;

        var dialog = new SaveFileDialog
        {
            Title = "Exportar relatório (HTML)",
            Filter = "Página HTML (*.html)|*.html",
            FileName = $"relatorio_{_account.Number}_{DateTime.Now:yyyyMMdd}.html"
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            File.WriteAllText(dialog.FileName, BuildHtml(), new UTF8Encoding(true));
            _dialog.Info("Relatório exportado. Abra no navegador e use \"Imprimir > Salvar como PDF\" se quiser um PDF.");
        }
        catch (Exception ex)
        {
            _dialog.Error("Não foi possível salvar o arquivo: " + ex.Message);
        }
    }

    private void ExportXlsx()
    {
        if (_account is null) return;

        var dialog = new SaveFileDialog
        {
            Title = "Exportar relatório (Excel)",
            Filter = "Planilha Excel (*.xlsx)|*.xlsx",
            FileName = $"relatorio_{_account.Number}_{DateTime.Now:yyyyMMdd}.xlsx"
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            using var wb = new XLWorkbook();

            // ---- Aba Resumo ----
            var resumo = wb.Worksheets.Add("Resumo");
            resumo.Cell(1, 1).Value = "PagnanBank - Relatório Financeiro";
            resumo.Cell(1, 1).Style.Font.Bold = true;
            resumo.Cell(1, 1).Style.Font.FontSize = 16;
            resumo.Cell(2, 1).Value = "Titular:"; resumo.Cell(2, 2).Value = OwnerName;
            resumo.Cell(3, 1).Value = "Conta:"; resumo.Cell(3, 2).Value = AccountNumber;
            resumo.Cell(4, 1).Value = "Gerado em:";
            resumo.Cell(4, 2).Value = DateTime.Now;
            resumo.Cell(4, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

            var r = 6;
            resumo.Cell(r, 1).Value = "Resumo";
            resumo.Cell(r, 1).Style.Font.Bold = true;
            r++;

            void Kv(string label, decimal value)
            {
                resumo.Cell(r, 1).Value = label;
                var cell = resumo.Cell(r, 2);
                cell.Value = value;
                cell.Style.NumberFormat.Format = "#,##0.00";
                r++;
            }

            Kv("Saldo atual", Balance);
            Kv("Total investido", TotalInvested);
            Kv("Empréstimos em aberto", LoansOutstanding);
            Kv("Entradas (mês)", MonthIncome);
            Kv("Saídas (mês)", MonthExpense);
            Kv("Cashback acumulado", TotalCashback);
            resumo.Cell(r, 1).Value = "Movimentações";
            resumo.Cell(r, 2).Value = TransactionCount;
            resumo.Columns().AdjustToContents();

            // ---- Aba Extrato ----
            var ws = wb.Worksheets.Add("Extrato");
            var headers = new[] { "Data", "Hora", "Operação", "Descrição", "Tipo", "Entrada (R$)", "Saída (R$)", "Saldo após (R$)" };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F4C81");
            }

            var row = 2;
            foreach (var t in _statement)
            {
                ws.Cell(row, 1).Value = t.TimestampUtc;
                ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(row, 2).Value = t.TimestampUtc;
                ws.Cell(row, 2).Style.DateFormat.Format = "HH:mm";
                ws.Cell(row, 3).Value = t.TypeLabel;
                ws.Cell(row, 4).Value = t.Description;
                ws.Cell(row, 5).Value = t.IsCredit ? "Crédito" : "Débito";
                if (t.IsCredit)
                {
                    ws.Cell(row, 6).Value = t.Amount;
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                }
                else
                {
                    ws.Cell(row, 7).Value = t.Amount;
                    ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                }
                ws.Cell(row, 8).Value = t.BalanceAfter;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
                row++;
            }

            var totalCredits = _statement.Where(t => t.IsCredit).Sum(t => t.Amount);
            var totalDebits = _statement.Where(t => !t.IsCredit).Sum(t => t.Amount);
            ws.Cell(row + 1, 5).Value = "Totais";
            ws.Cell(row + 1, 5).Style.Font.Bold = true;
            ws.Cell(row + 1, 6).Value = totalCredits;
            ws.Cell(row + 1, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row + 1, 7).Value = totalDebits;
            ws.Cell(row + 1, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();

            wb.SaveAs(dialog.FileName);
            _dialog.Info("Relatório Excel exportado com sucesso.");
        }
        catch (Exception ex)
        {
            _dialog.Error("Não foi possível salvar o arquivo: " + ex.Message);
        }
    }

    private void ExportPdf()
    {
        if (_account is null) return;

        var dialog = new SaveFileDialog
        {
            Title = "Exportar relatório (PDF)",
            Filter = "Documento PDF (*.pdf)|*.pdf",
            FileName = $"relatorio_{_account.Number}_{DateTime.Now:yyyyMMdd}.pdf"
        };
        if (dialog.ShowDialog() != true)
            return;

        var culture = CultureInfo.GetCultureInfo("pt-BR");
        string Money(decimal v) => v.ToString("C", culture);
        var totalCredits = _statement.Where(t => t.IsCredit).Sum(t => t.Amount);
        var totalDebits = _statement.Where(t => !t.IsCredit).Sum(t => t.Amount);

        try
        {
            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                    page.Header().Column(h =>
                    {
                        h.Item().Text("PagnanBank").FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                        h.Item().Text("Relatório Financeiro").FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);
                        h.Item().Text($"Titular: {OwnerName}  |  Conta: {AccountNumber}  |  Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Row(row =>
                        {
                            SummaryCard(row, "Saldo atual", Money(Balance));
                            SummaryCard(row, "Total investido", Money(TotalInvested));
                            SummaryCard(row, "Empréstimos", Money(LoansOutstanding));
                        });
                        col.Item().Row(row =>
                        {
                            SummaryCard(row, "Entradas (mês)", Money(MonthIncome));
                            SummaryCard(row, "Saídas (mês)", Money(MonthExpense));
                            SummaryCard(row, "Cashback", Money(TotalCashback));
                        });

                        col.Item().PaddingTop(6).Text($"Extrato ({TransactionCount} movimentações)").FontSize(13).SemiBold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(90);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1.4f);
                                c.RelativeColumn(1.4f);
                            });

                            table.Cell().Element(HeaderCell).Text("Data/Hora");
                            table.Cell().Element(HeaderCell).Text("Operação");
                            table.Cell().Element(HeaderCell).Text("Descrição");
                            table.Cell().Element(HeaderCell).Text("Valor");
                            table.Cell().Element(HeaderCell).Text("Saldo após");

                            foreach (var t in _statement)
                            {
                                var color = t.IsCredit ? Colors.Green.Darken1 : Colors.Red.Darken1;
                                var signed = (t.IsCredit ? "+ " : "- ") + Money(t.Amount);
                                table.Cell().Element(BodyCell).Text($"{t.TimestampUtc:dd/MM/yy HH:mm}");
                                table.Cell().Element(BodyCell).Text(t.TypeLabel);
                                table.Cell().Element(BodyCell).Text(t.Description);
                                table.Cell().Element(BodyCell).AlignRight().Text(signed).FontColor(color);
                                table.Cell().Element(BodyCell).AlignRight().Text(Money(t.BalanceAfter));
                            }
                        });

                        col.Item().AlignRight().Text($"Entradas: {Money(totalCredits)}    Saídas: {Money(totalDebits)}").SemiBold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("PagnanBank — página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(dialog.FileName);

            _dialog.Info("Relatório PDF exportado com sucesso.");
        }
        catch (Exception ex)
        {
            _dialog.Error("Não foi possível gerar o PDF: " + ex.Message);
        }
    }

    private static void SummaryCard(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
        {
            c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Medium);
            c.Item().Text(value).FontSize(12).SemiBold();
        });
    }

    private static IContainer HeaderCell(IContainer container)
        => container.Background(Colors.Blue.Darken3).Padding(5).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());

    private static IContainer BodyCell(IContainer container)
        => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(4);

    private string BuildHtml()
    {
        string Money(decimal v) => v.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));

        var rows = new StringBuilder();
        foreach (var t in _statement)
        {
            var color = t.IsCredit ? "#2EA36B" : "#C0392B";
            var signed = (t.IsCredit ? "+ " : "- ") + Money(t.Amount);
            rows.Append($"<tr><td>{t.TimestampUtc:dd/MM/yyyy HH:mm}</td><td>{WebEncode(t.TypeLabel)}</td>" +
                        $"<td>{WebEncode(t.Description)}</td><td style='color:{color};text-align:right'>{signed}</td>" +
                        $"<td style='text-align:right'>{Money(t.BalanceAfter)}</td></tr>");
        }

        return $@"<!DOCTYPE html>
<html lang='pt-BR'><head><meta charset='utf-8'>
<title>PagnanBank - Relatório - {WebEncode(OwnerName)}</title>
<style>
 body {{ font-family: Segoe UI, Arial, sans-serif; color:#1F2933; margin:40px; }}
 h1 {{ color:#0F4C81; }} .muted {{ color:#7B8794; }}
 .cards {{ display:flex; gap:16px; flex-wrap:wrap; margin:20px 0; }}
 .card {{ border:1px solid #D9DEE4; border-radius:10px; padding:16px; min-width:170px; }}
 .card .label {{ color:#7B8794; font-size:12px; }} .card .value {{ font-size:20px; font-weight:600; }}
 table {{ width:100%; border-collapse:collapse; margin-top:16px; }}
 th,td {{ padding:8px 10px; border-bottom:1px solid #EEF1F5; font-size:14px; text-align:left; }}
 th {{ background:#F4F6F9; }}
</style></head><body>
<h1>PagnanBank</h1>
<p class='muted' style='font-size:15px;color:#0F4C81;margin-top:-10px'>Relatório Financeiro</p>
<p class='muted'>Titular: {WebEncode(OwnerName)} &nbsp;|&nbsp; Conta: {WebEncode(AccountNumber)} &nbsp;|&nbsp; Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</p>
<div class='cards'>
 <div class='card'><div class='label'>Saldo atual</div><div class='value'>{Money(Balance)}</div></div>
 <div class='card'><div class='label'>Total investido</div><div class='value'>{Money(TotalInvested)}</div></div>
 <div class='card'><div class='label'>Empréstimos em aberto</div><div class='value'>{Money(LoansOutstanding)}</div></div>
 <div class='card'><div class='label'>Entradas (mês)</div><div class='value'>{Money(MonthIncome)}</div></div>
 <div class='card'><div class='label'>Saídas (mês)</div><div class='value'>{Money(MonthExpense)}</div></div>
 <div class='card'><div class='label'>Cashback acumulado</div><div class='value'>{Money(TotalCashback)}</div></div>
</div>
<h2>Extrato ({TransactionCount} movimentações)</h2>
<table><thead><tr><th>Data/Hora</th><th>Operação</th><th>Descrição</th><th>Valor</th><th>Saldo após</th></tr></thead>
<tbody>{rows}</tbody></table>
</body></html>";
    }

    private static string Escape(string value)
        => value.Contains(';') || value.Contains('"')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;

    private static string WebEncode(string value)
        => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
