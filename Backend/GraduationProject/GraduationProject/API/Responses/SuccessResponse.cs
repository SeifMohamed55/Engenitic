using System.Net;
using System.Text.Json.Serialization;

namespace GraduationProject.API.Responses
{

    public interface IResponse
    {

    }

    public class SuccessResponse : IResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Success";

        [JsonPropertyName("code")]
        public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }
}
