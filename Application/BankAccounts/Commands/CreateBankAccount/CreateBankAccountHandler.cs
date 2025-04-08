using Application.BankAccounts.Dtos;
using Application.BankAccounts.Interfaces;
using AutoMapper;
using Domain.Models.Accounts;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Commands.CreateBankAccount
{
    public class CreateBankAccountHandler : IRequestHandler<CreateBankAccountCommand, OperationResult<BankAccountDtoResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IBankAccountRepository _repo;

        public CreateBankAccountHandler(IMapper mapper, IBankAccountRepository repo)
        {
            _mapper = mapper;
            _repo = repo;
        }

        public async Task<OperationResult<BankAccountDtoResponse>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
        {
            // Map DTO to Entity
            var entity = _mapper.Map<BankAccount>(request.Dto);

            // Generate account number (basic logic — replace with something more robust later)
            entity.AccountNumber = $"ACC-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            await _repo.AddAsync(entity);

            var resultDto = _mapper.Map<BankAccountDtoResponse>(entity);
            return OperationResult<BankAccountDtoResponse>.Success(resultDto);
        }
    }
}
