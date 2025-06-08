using Microsoft.AspNetCore.Identity;
using Sprache;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace GraduationProject.Application.Services
{

    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }  // Data is only available for success
        public string Message { get; } 
        public HttpStatusCode StatusCode { get; }

        public IDictionary<string, string[]>? Errors { get; } = null;

        private ServiceResult(bool isSuccess, T? data, string message, HttpStatusCode code, IDictionary<string, string[]>? errors)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            StatusCode = code;
            Errors = errors;
        }


        public static ServiceResult<T> Success(T data, string message, HttpStatusCode code = HttpStatusCode.OK) 
            => new(true, data, message, code, null);

        public static ServiceResult<T> Failure(string message, IEnumerable<IdentityError> errors, HttpStatusCode code = HttpStatusCode.BadRequest)
        {
         var errorDictionary = errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,          // The error code as key
                g => g.Select(e => e.Description).ToArray()  // Array of descriptions
            );
            return new(false, default, message, code, errorDictionary);
        }
            

        public static ServiceResult<T> Failure(string message, HttpStatusCode code = HttpStatusCode.BadRequest) 
            => new(false, default, message, code, null);

        public bool TryGetData([NotNullWhen(true)] out T? data)
        {
            data = Data;
            return IsSuccess && data != null;
        }

        public bool TryGetErrors([NotNullWhen(true)] out IDictionary<string, string[]>? errors)
        {
            errors = Errors;
            return !IsSuccess && errors != null;
        }
    }


}
