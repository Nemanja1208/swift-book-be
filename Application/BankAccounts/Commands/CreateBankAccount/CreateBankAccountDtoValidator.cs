using FluentValidation;

namespace Application.BankAccounts.Commands.CreateBankAccount
{
    public class CreateBankAccountDtoValidator : AbstractValidator<CreateBankAccountCommand>
    {
        public CreateBankAccountDtoValidator()
        {
            RuleFor(x => x.Dto.OwnerName)
                .NotEmpty().WithMessage("Owner name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Dto.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter code.");
        }
    }
}
