using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Common.Extensions
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult<T>(this ServiceResult<T> result)
        {
            if (result.TryGetData(out var data))
                return new OkObjectResult(new SuccessResponse
                {
                    Data = data,
                    Message = result.Message,
                    Code = result.StatusCode,
                });
            
            else
                return new ObjectResult(new ErrorResponse
                {
                    Message = result.Message,
                    Errors = result.TryGetErrors(out var errors) ? errors : null,
                })
                {
                    StatusCode = (int)result.StatusCode
                };
        }
    }
}
