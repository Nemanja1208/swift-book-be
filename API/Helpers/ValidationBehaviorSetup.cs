using Domain.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Helpers
{
    public static class ValidationBehaviorSetup
    {
        public static IServiceCollection AddCustomValidationResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();

                    var result = OperationResult<string>.Failure(errors);
                    return new BadRequestObjectResult(result);
                };
            });

            return services;
        }
    }
}
