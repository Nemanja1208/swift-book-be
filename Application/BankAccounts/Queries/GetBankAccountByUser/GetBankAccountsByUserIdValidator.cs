using FluentValidation;

namespace Application.BankAccounts.Queries.GetBankAccountByUser
{
    public class GetBankAccountsByUserIdQueryValidator : AbstractValidator<GetBankAccountsByUserIdQuery>
    {
        public GetBankAccountsByUserIdQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User Id is required.");
        }
    }
}
