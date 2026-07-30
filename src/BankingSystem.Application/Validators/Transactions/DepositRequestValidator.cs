using BankingSystem.Application.DTOs.Transactions;
using FluentValidation;

namespace BankingSystem.Application.Validators.Transactions;

public sealed class DepositRequestValidator : AbstractValidator<DepositRequest>
{
    public DepositRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
    }
}
