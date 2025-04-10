using Application.BankAccounts.Dtos;
using Application.BankAccounts.Interfaces;
using AutoMapper;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Commands.UpdateBankAccount
{
    public class UpdateBankAccountHandler : IRequestHandler<UpdateBankAccountCommand, OperationResult<BankAccountDtoResponse>>
    {
        private readonly IBankAccountRepository _repo;
        private readonly IMapper _mapper;

        public UpdateBankAccountHandler(IBankAccountRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<BankAccountDtoResponse>> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity is null)
                return OperationResult<BankAccountDtoResponse>.Failure("Account not found.");

            _mapper.Map(request.Dto, entity); // in-place update

            await _repo.UpdateAsync(entity);

            var dto = _mapper.Map<BankAccountDtoResponse>(entity);
            return OperationResult<BankAccountDtoResponse>.Success(dto);
        }
    }

}
