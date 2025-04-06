using System.Diagnostics.CodeAnalysis;

namespace GraduationProject.Application.Services
{

    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }  // Data is only available for success
        public string Message { get; }  // Error is only available for failure

        private ServiceResult(bool isSuccess, T? data, string message)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
        }


        public static ServiceResult<T> Success(T data) => new(true, data, "Service completed successfully.");

        public static ServiceResult<T> Failure(string error) => new(false, default, error);
        public static ServiceResult<T> Failure(ICollection<string> errors) => new(false, default, string.Join('\n', errors));

        public bool TryGetData([NotNullWhen(true)] out T? data)
        {
            data = Data;
            return IsSuccess && data != null;
        }

        public bool TryGetError([NotNullWhen(true)] out string? error)
        {
            error = Message;
            return !IsSuccess && error != null;
        }
    }


}
