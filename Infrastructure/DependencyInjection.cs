using Application.BankAccounts.Interfaces;
using Application.Common.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Database;
using Infrastructure.Database.Seeding;
using Infrastructure.Interceptors;
using Infrastructure.Repositories;
using Infrastructure.Repositories.BankAccounts;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings")
            );

            var connectionString = env.IsDevelopment()
            ? configuration.GetConnectionString("DefaultConnection")
            : configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");


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
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DataSeeder.SeedAsync(db).GetAwaiter().GetResult(); // ✅ sync-safe


            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextService, UserContextService>();

            return services;
        }
    }
}
