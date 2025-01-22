namespace GraduationProject.Models.DTOs
{
     public class AppUserDto
        {
            public int Id { get; set; }
            public string RefreshToken { get; set; } = null!;
            public DateTimeOffset ExpiryDate { get; set; }
            public string Email { get; set; } = null!;
            public string? PhoneNumber { get; set; } = null!;
            public string? PhoneRegionCode { get; set; } = null!;
            public string? ImageURL { get; set; } = null!;
            public IEnumerable<string> Roles { get; set; } = null!;
        }
}
