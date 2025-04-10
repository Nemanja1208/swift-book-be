using Application.BankAccounts.Dtos;
using Application.BankAccounts.Interfaces;
using AutoMapper;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Queries.GetById
{
    public class GetBankAccountByIdHandler : IRequestHandler<GetBankAccountByIdQuery, OperationResult<BankAccountDtoResponse>>
    {
        private readonly IBankAccountRepository _repo;
        private readonly IMapper _mapper;

        public GetBankAccountByIdHandler(IBankAccountRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<BankAccountDtoResponse>> Handle(GetBankAccountByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);

            if (entity is null)
                return OperationResult<BankAccountDtoResponse>.Failure("Bank account not found.");

            var dto = _mapper.Map<BankAccountDtoResponse>(entity);
            return OperationResult<BankAccountDtoResponse>.Success(dto);
        }
    }
}
