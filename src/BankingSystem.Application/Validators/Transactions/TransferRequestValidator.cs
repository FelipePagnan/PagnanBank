using BankingSystem.Application.DTOs.Transactions;
using FluentValidation;

namespace BankingSystem.Application.Validators.Transactions;

public sealed class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.SourceAccountId).NotEmpty();
        RuleFor(x => x.DestinationAccountNumber)
            .NotEmpty().WithMessage("Informe a conta de destino.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
    }
}
