using Domain.Models;
using Domain.Models.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<LogEntry> Logs => Set<LogEntry>();

    }
}
