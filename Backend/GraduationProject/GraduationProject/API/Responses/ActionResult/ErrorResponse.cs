using System.Net;
using System.Text.Json.Serialization;

namespace GraduationProject.API.Responses.ActionResult
{
    public class ErrorResponse : IApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Error";

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("code")]
        public HttpStatusCode Code { get; set; } = HttpStatusCode.BadRequest;

        [JsonPropertyName("errors")]
        public IDictionary<string, string[]>? Errors { get; set; } = null;

    }
}
