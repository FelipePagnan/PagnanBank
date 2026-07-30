using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Common.Errors;

/// <summary>Central catalogue of business errors returned by the service layer.</summary>
public static class DomainErrors
{
    public static class Auth
    {
        public static readonly Error InvalidCredentials =
            new("Auth.InvalidCredentials", "E-mail ou senha inválidos.");

        public static readonly Error AccountLocked =
            new("Auth.AccountLocked", "Usuário bloqueado por excesso de tentativas. Contate o administrador.");

        public static readonly Error UserInactive =
            new("Auth.UserInactive", "Usuário inativo.");

        public static readonly Error Forbidden =
            new("Auth.Forbidden", "Acesso negado. Esta operação é restrita a administradores.");
    }

    public static class Users
    {
        public static readonly Error NotFound = new("User.NotFound", "Usuário não encontrado.");
        public static readonly Error EmailInUse = new("User.EmailInUse", "E-mail já cadastrado.");
        public static readonly Error CpfInUse = new("User.CpfInUse", "CPF já cadastrado.");
        public static readonly Error CannotDeleteSelf = new("User.CannotDeleteSelf", "Você não pode excluir o próprio usuário.");
        public static readonly Error CannotDeleteAdmin = new("User.CannotDeleteAdmin", "Não é possível excluir um administrador.");
    }

    public static class Accounts
    {
        public static readonly Error NotFound = new("Account.NotFound", "Conta não encontrada.");
        public static readonly Error Blocked = new("Account.Blocked", "Conta bloqueada.");
        public static readonly Error InsufficientFunds = new("Account.InsufficientFunds", "Saldo insuficiente.");
        public static readonly Error SameAccount = new("Account.SameAccount", "A conta de origem e destino não podem ser a mesma.");
        public static readonly Error DestinationNotFound = new("Account.DestinationNotFound", "Conta de destino não encontrada.");
    }

    public static class Transactions
    {
        public static readonly Error InvalidAmount = new("Transaction.InvalidAmount", "O valor deve ser maior que zero.");
    }

    public static class Investments
    {
        public static readonly Error NotFound = new("Investment.NotFound", "Investimento não encontrado.");
        public static readonly Error AlreadyRedeemed = new("Investment.AlreadyRedeemed", "Este investimento já foi resgatado.");
    }

    public static class Loans
    {
        public static readonly Error NotFound = new("Loan.NotFound", "Empréstimo não encontrado.");
        public static readonly Error AlreadySettled = new("Loan.AlreadySettled", "Este empréstimo já está quitado.");
        public static readonly Error InvalidInstallments = new("Loan.InvalidInstallments", "O número de parcelas deve ser maior que zero.");
    }

    public static class Cards
    {
        public static readonly Error NotFound = new("Card.NotFound", "Cartão não encontrado.");
        public static readonly Error Blocked = new("Card.Blocked", "Cartão bloqueado.");
        public static readonly Error LimitExceeded = new("Card.LimitExceeded", "Limite do cartão insuficiente.");
        public static readonly Error LimitBelowUsed = new("Card.LimitBelowUsed", "O novo limite não pode ser menor que o valor já utilizado.");
        public static readonly Error NoInvoice = new("Card.NoInvoice", "Não há fatura em aberto neste cartão.");
    }

    public static class Store
    {
        public static readonly Error EmptyCart = new("Store.EmptyCart", "O carrinho está vazio.");
        public static readonly Error ProductNotFound = new("Store.ProductNotFound", "Produto não encontrado.");
        public static readonly Error CardRequired = new("Store.CardRequired", "Selecione um cartão para pagar no crédito.");
    }

    public static class Validation
    {
        public static Error Rule(string message) => new("Validation", message);
    }
}
