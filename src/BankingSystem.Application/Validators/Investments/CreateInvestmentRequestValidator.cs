using BankingSystem.Application.DTOs.Investments;
using FluentValidation;

namespace BankingSystem.Application.Validators.Investments;

public sealed class CreateInvestmentRequestValidator : AbstractValidator<CreateInvestmentRequest>
{
    public CreateInvestmentRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Principal).GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
        RuleFor(x => x.AnnualRatePercent).GreaterThanOrEqualTo(0).WithMessage("A taxa não pode ser negativa.");
    }
}
