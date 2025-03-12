namespace GraduationProject.Domain.DTOs
{
    public class AppUserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string? PhoneRegionCode { get; set; } = null!;
        public ImageMetadata Image { get; set; } = null!;
        public bool Banned { get; set; }
        public bool IsExternal { get; set; }
    }
}
