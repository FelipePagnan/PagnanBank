using BankingSystem.Application.DTOs.Users;
using BankingSystem.Domain.Common;
using FluentValidation;

namespace BankingSystem.Application.Validators.Users;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Informe o nome completo.")
            .MinimumLength(3).WithMessage("Nome muito curto.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("Informe o CPF.")
            .Must(CpfValidator.IsValid)
            .WithMessage("CPF inválido.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Informe a senha.")
            .MinimumLength(6).WithMessage("A senha deve ter ao menos 6 caracteres.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("O saldo inicial não pode ser negativo.");
    }
}
