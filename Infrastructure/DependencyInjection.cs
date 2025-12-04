using Application.BankAccounts.Interfaces;
using Application.Common.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Database;
using Infrastructure.Interceptors;
using Infrastructure.Repositories;
using Infrastructure.Repositories.BankAccounts;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings")
            );

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var logger = loggerFactory.CreateLogger("Infrastructure.Startup");
    
            logger.LogInformation("Environment: {Env}", env.EnvironmentName);
            logger.LogInformation("IsDevelopment: {IsDev}", env.IsDevelopment());
    
            // Azure SQLAzure connection strings are prefixed with SQLAZURECONNSTR_
            // Try the standard way first, then fall back to Azure's environment variable
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
                ?? throw new InvalidOperationException($"Connection string 'DefaultConnection' not found for environment '{env.EnvironmentName}'");

            logger.LogInformation("DefaultConnection exists: {Exists}", !string.IsNullOrEmpty(connectionString));

            // Don't log the actual connection string in production! (contains password)
            // But you can log a masked version:
            var server = connectionString.Split(';')
                .FirstOrDefault(s => s.StartsWith("Server=", StringComparison.OrdinalIgnoreCase));
            logger.LogInformation("Server: {Server}", server ?? "not found");



            services.AddScoped<IAuthService, AuthService>();

            services.AddSingleton<SaveChangesInterceptor, LogSaveChangesInterceptor>();

            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<SaveChangesInterceptor>();

                options.UseSqlServer(connectionString);

                options.AddInterceptors(interceptor);
            });


            // Register generic repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register feature-specific repositories
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();

            // Seeding
            //using var provider = services.BuildServiceProvider();
            //using var scope = provider.CreateScope();

            //var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //DataSeeder.SeedAsync(db).GetAwaiter().GetResult(); // ✅ sync-safe


            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextService, UserContextService>();

            return services;
        }
    }
}
