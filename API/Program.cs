using API.Helpers;
using API.Middleware;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
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

            // Get CORS origins from configuration or use defaults
            var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                ?? new[] { "http://localhost:5173", "http://localhost:8080", "https://nbihak.netlify.app" };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(corsOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var app = builder.Build();

            // Configure forwarded headers for Azure (handles X-Forwarded-* headers from load balancer)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Only enable Swagger in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Only redirect to HTTPS in development (Azure handles HTTPS at the edge)
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Enable CORS (must be before Authentication/Authorization)
            app.UseCors("AllowFrontend");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
