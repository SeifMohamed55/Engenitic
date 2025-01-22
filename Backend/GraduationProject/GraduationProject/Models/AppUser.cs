
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser<int>
{
    [ProtectedPersonalData]
    public override string Email { get; set; } = null!;
    public string? PhoneRegionCode { get; set; }
    public string? imageURL { get; set; }
    public bool Banned { get; set; }

    public int? RefreshTokenId { get; set; }
    public RefreshToken? RefreshToken { get; set; } = null!;

    public ICollection<Role> Roles { get; set; } = new List<Role>();

    public void UpdateFromDTO(AppUserDto dto)
    {
        Id = dto.Id;
        Email = dto.Email;
        PhoneNumber = dto.PhoneNumber;
        PhoneRegionCode = dto.PhoneRegionCode;
        imageURL = dto.ImageURL;
    }
}
