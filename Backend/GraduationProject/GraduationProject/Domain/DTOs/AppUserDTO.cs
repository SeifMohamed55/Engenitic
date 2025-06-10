namespace GraduationProject.Domain.DTOs
{
    public class AppUserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string? PhoneRegionCode { get; set; } = null!;
        public FileMetadata Image { get; set; } = null!;
        public FileMetadata Cv { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Banned { get; set; }
        public bool IsExternal { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public ICollection<string> Roles { get; set; } = [];
    }
}
