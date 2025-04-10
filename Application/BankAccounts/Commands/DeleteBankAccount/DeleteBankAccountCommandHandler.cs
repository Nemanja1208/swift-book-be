using Application.BankAccounts.Interfaces;
using Domain.Models.Common;
using MediatR;

namespace Application.BankAccounts.Commands.DeleteBankAccount
{
    public class DeleteBankAccountHandler : IRequestHandler<DeleteBankAccountCommand, OperationResult<string>>
    {
        private readonly IBankAccountRepository _repo;

        public DeleteBankAccountHandler(IBankAccountRepository repo)
        {
            _repo = repo;
        }

        public async Task<OperationResult<string>> Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity is null)
                return OperationResult<string>.Failure("Account not found.");

            await _repo.DeleteAsync(entity);

            return OperationResult<string>.Success("Deleted successfully.");
        }
    }

}
