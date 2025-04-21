using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using System.Net;

namespace GraduationProject.Common.Middlewares
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public TokenBlacklistMiddleware
            (RequestDelegate next,
            ITokenBlacklistService tokenBlacklistService,
            IJwtTokenService jwtTokenService
            )
        {
            _next = next;
            _tokenBlacklistService = tokenBlacklistService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_tokenBlacklistService.IsTokenBlacklisted(context))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ErrorResponse()
                {
                    Code = HttpStatusCode.Unauthorized,
                    Message = "Token has been blacklisted Logging User out."
                });

                return;
            }

            await _next(context);
        }
    }

}
