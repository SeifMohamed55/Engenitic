
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser<int>
{
    [ProtectedPersonalData]
    public override string Email { get; set; } = null!;
    public string? PhoneRegionCode { get; set; }
    public string ImageSrc { get; set; } = null!;
    public bool Banned { get; set; }
    public string FullName { get; set; } = null!;

    public int? RefreshTokenId { get; set; }
    public RefreshToken? RefreshToken { get; set; } = null!;

    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();

    public void UpdateFromDTO(AppUserDTO dto)
    {
        Id = dto.Id;
        Email = dto.Email;
        PhoneNumber = dto.PhoneNumber;
        PhoneRegionCode = dto.PhoneRegionCode;
        FullName = dto.UserName;
    }
}
