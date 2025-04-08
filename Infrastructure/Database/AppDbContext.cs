using System.Text.Json;
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

        // Add more DbSets here...
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var logs = new List<LogEntry>();

            foreach (var entry in ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Added ||
                                     e.State == EntityState.Modified ||
                                     e.State == EntityState.Deleted))
            {
                var entityName = entry.Entity.GetType().Name;
                var operation = entry.State.ToString();

                var serialized = JsonSerializer.Serialize(entry.Entity, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });

                logs.Add(new LogEntry
                {
                    EntityName = entityName,
                    Operation = operation,
                    Data = serialized,
                    Timestamp = DateTime.UtcNow
                });
            }

            // First save the changes to your actual entities
            var result = await base.SaveChangesAsync(cancellationToken);

            // Then save the logs
            if (logs.Any())
            {
                Logs.AddRange(logs);
                await base.SaveChangesAsync(cancellationToken); // avoid recursion!
            }

            return result;
        }
    }
}
