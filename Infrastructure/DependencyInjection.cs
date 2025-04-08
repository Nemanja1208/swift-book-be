using Application.BankAccounts.Interfaces;
using Application.Common.Interfaces;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Infrastructure.Repositories.BankAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register generic repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register feature-specific repositories
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();

            return services;
        }
    }
}
