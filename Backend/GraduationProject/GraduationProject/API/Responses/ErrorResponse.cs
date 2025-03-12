using System.Net;
using System.Text.Json.Serialization;

namespace GraduationProject.API.Responses
{
    public class ErrorResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Error";

        [JsonPropertyName("code")]
        public HttpStatusCode Code { get; set; }

        [JsonPropertyName("message")]
        public object Message { get; set; } = null!;

    }
}
