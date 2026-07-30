namespace BankingSystem.Domain.Enums;

public enum TransactionType
{
    Deposit = 0,             // Depósito
    Withdraw = 1,            // Saque
    TransferOut = 2,         // Transferência enviada
    TransferIn = 3,          // Transferência recebida
    PixOut = 4,              // PIX enviado
    PixIn = 5,               // PIX recebido
    AdminCredit = 6,         // Ajuste administrativo (crédito)
    AdminDebit = 7,          // Ajuste administrativo (débito)
    InvestmentBuy = 8,       // Aplicação em investimento (débito)
    InvestmentRedeem = 9,    // Resgate de investimento (crédito)
    LoanCredit = 10,         // Liberação de empréstimo (crédito)
    LoanPayment = 11,        // Pagamento de parcela (débito)
    Purchase = 12,           // Compra na loja em débito (débito)
    Cashback = 13,           // Cashback de compra (crédito)
    CardInvoicePayment = 14  // Pagamento de fatura do cartão (débito)
}
