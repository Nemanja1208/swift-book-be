using Application.BankAccounts.Dtos;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Queries.GetBankAccountByUser
{
    public record GetBankAccountsByUserIdQuery(Guid UserId)
    : IRequest<OperationResult<List<BankAccountDtoResponse>>>;

}
