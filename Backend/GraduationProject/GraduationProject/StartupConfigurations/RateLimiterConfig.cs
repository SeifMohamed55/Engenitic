using GraduationProject.Controllers.APIResponses;
using System.Threading.RateLimiting;

namespace GraduationProject.StartupConfigurations
{
    public static class RateLimiterConfig
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            return services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3000,
                        Window = TimeSpan.FromMinutes(5)
                    });
                });

                options.AddPolicy("UserLoginRateLimit", context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous"; // Get user Ip Address
                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    });
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    // Default retry time (fallback)
                    var retryAfter = TimeSpan.FromMinutes(1);

                    // Retrieve rate-limiting metadata (if available)
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryTimeMetadata))
                    {
                        retryAfter = retryTimeMetadata;
                    }

                    // Set headers dynamically
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    var errorResponse = new ErrorResponse()
                    {
                        Code = System.Net.HttpStatusCode.TooManyRequests,
                        Message = $"Too many requests, try again in {retryAfter.TotalSeconds} seconds."
                    };

                    try
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken: token);
                    }
                    catch (Exception)
                    {
                    }
                };
            });
        }
    }
}
