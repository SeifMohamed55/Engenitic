
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser<int>
{
    public bool Banned { get; set; }

    [ProtectedPersonalData]
    public override string Email { get; set; } = null!;
    public string? PhoneRegionCode { get; set; }
    public string? imageURL { get; set; }

    public ICollection<Role> Roles { get; set; } = new List<Role>();

    public void UpdateFromDTO(AppUserDTO dto)
    {
        Id = dto.Id;
        Email = dto.Email;
        PhoneNumber = dto.PhoneNumber;
    }
}
