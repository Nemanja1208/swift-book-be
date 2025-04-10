using Application.Common.Interfaces;
using Domain.Models.Accounts;

namespace Application.BankAccounts.Interfaces
{
    public interface IBankAccountRepository : IGenericRepository<BankAccount>
    {
        Task<IEnumerable<BankAccount>> GetByUserIdAsync(Guid userId);
    }
}
