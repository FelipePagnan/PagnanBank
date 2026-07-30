using BankingSystem.Application.DTOs.Loans;
using FluentValidation;

namespace BankingSystem.Application.Validators.Loans;

public sealed class CreateLoanRequestValidator : AbstractValidator<CreateLoanRequest>
{
    public CreateLoanRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Principal).GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
        RuleFor(x => x.AnnualRatePercent).GreaterThanOrEqualTo(0).WithMessage("A taxa não pode ser negativa.");
        RuleFor(x => x.Installments).GreaterThan(0).WithMessage("Informe o número de parcelas.");
    }
}
