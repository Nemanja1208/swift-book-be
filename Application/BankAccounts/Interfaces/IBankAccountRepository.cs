using Application.Common.Interfaces;
using Domain.Models.Accounts;

namespace Application.BankAccounts.Interfaces
{
    public interface IBankAccountRepository : IGenericRepository<BankAccount>
    {
        // Add any custom methods like GetByAccountNumber, etc.
    }
}
