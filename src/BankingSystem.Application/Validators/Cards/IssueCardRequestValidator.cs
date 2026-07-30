using BankingSystem.Application.DTOs.Cards;
using FluentValidation;

namespace BankingSystem.Application.Validators.Cards;

public sealed class IssueCardRequestValidator : AbstractValidator<IssueCardRequest>
{
    public IssueCardRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Limit).GreaterThan(0).WithMessage("Informe um limite maior que zero.");
    }
}
