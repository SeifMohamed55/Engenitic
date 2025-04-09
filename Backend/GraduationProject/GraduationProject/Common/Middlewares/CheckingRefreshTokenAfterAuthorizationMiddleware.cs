using GraduationProject.API.Responses;
using System.Net;

namespace GraduationProject.Common.Middlewares
{
    public class CheckingRefreshTokenAfterAuthorizationMiddleware
    {

        private readonly RequestDelegate _next;

        public CheckingRefreshTokenAfterAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var requestRefToken = context.Request.Cookies["refreshToken"];
                if (requestRefToken == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new ErrorResponse
                    {
                        Message = "No Refresh Token Provided.",
                        Code = HttpStatusCode.Unauthorized
                    });
                    return;
                }
                // Example: Add a custom response header
            }

            await _next(context);
        }
    }
}
