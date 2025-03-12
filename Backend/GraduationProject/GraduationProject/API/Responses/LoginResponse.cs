using GraduationProject.Domain.DTOs;

namespace GraduationProject.API.Responses
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public bool Banned { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string ValidTo { get; set; } = string.Empty;
        public ImageMetadata Image { get; set; } = new();
        public string AccessToken { get; set; } = string.Empty;
    }
}
