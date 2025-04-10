using FluentValidation;

namespace Application.BankAccounts.Queries.GetById
{
    public class GetBankAccountByIdValidator : AbstractValidator<GetBankAccountByIdQuery>
    {
        public GetBankAccountByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
        }
    }
}
