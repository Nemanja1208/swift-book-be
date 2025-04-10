using Application.BankAccounts.Dtos;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Queries.GetAllBankAccounts
{
    public record GetAllBankAccountsQuery : IRequest<OperationResult<List<BankAccountDtoResponse>>>;

}
