using Application.BankAccounts.Dtos;
using Application.BankAccounts.Interfaces;
using Application.Common.Interfaces;
using AutoMapper;
using Domain.Models.Accounts;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Commands.CreateBankAccount
{
    public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, OperationResult<BankAccountDtoResponse>>
    {
        private readonly IBankAccountRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContext;

        public CreateBankAccountCommandHandler(
            IBankAccountRepository repo,
            IMapper mapper,
            IUserContextService userContext)
        {
            _repo = repo;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<OperationResult<BankAccountDtoResponse>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<BankAccount>(request.Dto);

            // ✅ Set the currently authenticated user's ID
            var userId = _userContext.UserId;
            if (string.IsNullOrEmpty(userId)) return OperationResult<BankAccountDtoResponse>.Failure("Invalid user context.");

            entity.UserId = Guid.Parse(userId);
            entity.AccountNumber = $"ACC-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            await _repo.AddAsync(entity);

            var dto = _mapper.Map<BankAccountDtoResponse>(entity);
            return OperationResult<BankAccountDtoResponse>.Success(dto);
        }
    }

}
