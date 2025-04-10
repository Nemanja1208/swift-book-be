using Application.BankAccounts.Interfaces;
using Domain.Models.Accounts;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.BankAccounts
{
    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<BankAccount>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }
    }
}
