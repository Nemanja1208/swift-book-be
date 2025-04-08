using Application.BankAccounts.Interfaces;
using Domain.Models.Accounts;
using Infrastructure.Database;

namespace Infrastructure.Repositories.BankAccounts
{
    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(AppDbContext context) : base(context) { }

        // Add any specific queries if needed later
    }
}
