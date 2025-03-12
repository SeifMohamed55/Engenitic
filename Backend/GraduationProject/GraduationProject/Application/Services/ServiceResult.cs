namespace GraduationProject.Application.Services
{

    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }  // Data is only available for success
        public string? Error { get; }  // Error is only available for failure

        private ServiceResult(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }


        public static ServiceResult<T> Success(T data) => new(true, data, null);

        public static ServiceResult<T> Failure(string error) => new(false, default, error);
        public static ServiceResult<T> Failure(ICollection<string> errors) => new(false, default, string.Join('\n', errors));
    }


}
