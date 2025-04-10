using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Commands.DeleteBankAccount
{
    public record DeleteBankAccountCommand(Guid Id)
    : IRequest<OperationResult<string>>;

}
