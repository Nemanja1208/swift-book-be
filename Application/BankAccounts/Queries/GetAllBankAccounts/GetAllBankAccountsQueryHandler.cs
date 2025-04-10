using Application.BankAccounts.Dtos;
using Application.BankAccounts.Interfaces;
using AutoMapper;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Queries.GetAllBankAccounts
{
    public class GetAllBankAccountsHandler : IRequestHandler<GetAllBankAccountsQuery, OperationResult<List<BankAccountDtoResponse>>>
    {
        private readonly IBankAccountRepository _repo;
        private readonly IMapper _mapper;

        public GetAllBankAccountsHandler(IBankAccountRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<List<BankAccountDtoResponse>>> Handle(GetAllBankAccountsQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetAllAsync();
            var dtoList = _mapper.Map<List<BankAccountDtoResponse>>(list);

            return OperationResult<List<BankAccountDtoResponse>>.Success(dtoList);
        }
    }
}
