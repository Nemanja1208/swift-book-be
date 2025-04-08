using Application.BankAccounts.Dtos;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Commands.CreateBankAccount
{
    public class CreateBankAccountCommand : IRequest<OperationResult<BankAccountDtoResponse>>
    {
        public CreateBankAccountDto Dto { get; }

        public CreateBankAccountCommand(CreateBankAccountDto dto)
        {
            Dto = dto;
        }
    }
}
