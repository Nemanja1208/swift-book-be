
using API.Helpers;
using API.Middleware;
using Application;
using Infrastructure;
using Microsoft.Extensions.Logging.AzureAppServices;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerWithJwtAuth();

            builder.Services.AddControllers();
            builder.Services.AddCustomValidationResponse();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Logging.AddAzureWebAppDiagnostics();

            // Add CORS configuration for frontend integration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:5173",  // Vite default dev server
                        "http://localhost:8080",  // Alternative React dev server
                        "https://nbihak.netlify.app/landing"  // Production domain (update as needed)
                    )
                    .AllowCredentials()           // Required for HttpOnly cookies
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            // Enable CORS (must be before Authentication/Authorization)
            app.UseCors("AllowFrontend");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
