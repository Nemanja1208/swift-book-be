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

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings")
            );

            services.AddScoped<IAuthService, AuthService>();

            services.AddSingleton<SaveChangesInterceptor, LogSaveChangesInterceptor>();

            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<SaveChangesInterceptor>();

                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                options.AddInterceptors(interceptor);
            });


            // Register generic repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register feature-specific repositories
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();

            return services;
        }
    }
}
