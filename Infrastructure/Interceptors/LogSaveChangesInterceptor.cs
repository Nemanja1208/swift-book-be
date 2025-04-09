using System.Text.Json;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Interceptors
{
    public class LogSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var logs = new List<LogEntry>();

            foreach (var entry in context.ChangeTracker.Entries()
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

            if (logs.Any())
            {
                context.Set<LogEntry>().AddRange(logs);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
