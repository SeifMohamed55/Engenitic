using GraduationProject.API.Requests;

namespace GraduationProject.API.Responses
{
    public class AuthenticatedPayload
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string ImageUrl { get; set; }
        public required string UniqueId { get; set; }
        public required DeviceInfo DeviceInfo{ get; set; }
    }
}
