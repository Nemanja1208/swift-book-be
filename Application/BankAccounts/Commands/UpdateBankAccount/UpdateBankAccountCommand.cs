using Application.BankAccounts.Dtos;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Commands.UpdateBankAccount
{
    public record UpdateBankAccountCommand(Guid Id, UpdateBankAccountDto Dto)
    : IRequest<OperationResult<BankAccountDtoResponse>>;

}
