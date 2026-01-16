using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddAppRateLimiter(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", fixedOptions =>
            {
                fixedOptions.PermitLimit = 10;
                fixedOptions.QueueLimit = 0;
                fixedOptions.Window = TimeSpan.FromSeconds(60);
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }
}