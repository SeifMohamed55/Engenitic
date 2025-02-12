using System.Net;

namespace GraduationProject.Controllers.APIResponses
{
    public class ErrorResponse
    {
        public string Status { get; set; } = "Error";
        public HttpStatusCode Code { get; set; }
        public object Message { get; set; } = null!;

    }
}
