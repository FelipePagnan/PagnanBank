using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.DTOs.Transactions;

public sealed class TransactionDto
{
    public Guid Id { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime TimestampUtc { get; init; }

    /// <summary>True when the movement increases the account balance.</summary>
    public bool IsCredit => Type is TransactionType.Deposit
        or TransactionType.TransferIn
        or TransactionType.PixIn
        or TransactionType.AdminCredit
        or TransactionType.InvestmentRedeem
        or TransactionType.LoanCredit
        or TransactionType.Cashback;

    /// <summary>Human-friendly operation name (pt-BR).</summary>
    public string TypeLabel => Type switch
    {
        TransactionType.Deposit => "Depósito",
        TransactionType.Withdraw => "Saque",
        TransactionType.TransferOut => "Transferência enviada",
        TransactionType.TransferIn => "Transferência recebida",
        TransactionType.PixOut => "PIX enviado",
        TransactionType.PixIn => "PIX recebido",
        TransactionType.AdminCredit => "Crédito administrativo",
        TransactionType.AdminDebit => "Débito administrativo",
        TransactionType.InvestmentBuy => "Aplicação",
        TransactionType.InvestmentRedeem => "Resgate de investimento",
        TransactionType.LoanCredit => "Empréstimo liberado",
        TransactionType.LoanPayment => "Pagamento de parcela",
        TransactionType.Purchase => "Compra na loja",
        TransactionType.Cashback => "Cashback",
        TransactionType.CardInvoicePayment => "Pagamento de fatura",
        _ => Type.ToString()
    };

    /// <summary>Signed, currency-formatted amount (e.g. "+ R$ 100,00").</summary>
    public string DisplayAmount => (IsCredit ? "+ " : "- ") + Amount.ToString("C");
}
