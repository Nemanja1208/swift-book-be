using Application.BankAccounts.Dtos;
using Application.BankAccounts.Interfaces;
using AutoMapper;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Queries.GetBankAccountByUser
{
    public class GetBankAccountsByUserIdHandler : IRequestHandler<GetBankAccountsByUserIdQuery, OperationResult<List<BankAccountDtoResponse>>>
    {
        private readonly IBankAccountRepository _repo;
        private readonly IMapper _mapper;

        public GetBankAccountsByUserIdHandler(IBankAccountRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<List<BankAccountDtoResponse>>> Handle(GetBankAccountsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _repo.GetByUserIdAsync(request.UserId);
            var dto = _mapper.Map<List<BankAccountDtoResponse>>(accounts);
            return OperationResult<List<BankAccountDtoResponse>>.Success(dto);
        }
    }

}
