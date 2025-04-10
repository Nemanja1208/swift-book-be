using Application.BankAccounts.Dtos;
using FluentValidation;

namespace Application.BankAccounts.Commands.UpdateBankAccount
{
    public class UpdateBankAccountDtoValidator : AbstractValidator<UpdateBankAccountDto>
    {
        public UpdateBankAccountDtoValidator()
        {
            RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
            RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
        }
    }
}
