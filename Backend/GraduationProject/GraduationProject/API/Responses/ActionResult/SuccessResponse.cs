using System.Net;
using System.Text.Json.Serialization;

namespace GraduationProject.API.Responses.ActionResult
{

    public class SuccessResponse : IApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Success";

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("code")]
        public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }
}
