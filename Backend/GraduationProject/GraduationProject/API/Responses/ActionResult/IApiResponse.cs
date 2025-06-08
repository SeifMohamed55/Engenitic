using System.Net;

namespace GraduationProject.API.Responses.ActionResult
{
    public interface IApiResponse
    {
        string Status { get; set; }
        string Message { get; set; }
        HttpStatusCode Code { get; set; }
    }
}
