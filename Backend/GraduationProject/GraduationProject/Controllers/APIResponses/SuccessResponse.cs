using System.Net;

namespace GraduationProject.Controllers.APIResponses
{
    public class SuccessResponse
    {
        public string Status { get; set; } = "Success";
        public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;
        public string Message { get; set; } = null!;
        public object? Data { get; set; }
    }
}
