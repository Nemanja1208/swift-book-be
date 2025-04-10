using Application.BankAccounts.Dtos;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Queries.GetById
{
    public record GetBankAccountByIdQuery(Guid Id) : IRequest<OperationResult<BankAccountDtoResponse>>;
}
