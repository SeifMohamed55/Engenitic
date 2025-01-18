
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser<int>
{
    [PersonalData]
    public string Address { get; set; } = null!;

    public string Country { get; set; } = null!;

    public bool Banned { get; set; }

    [ProtectedPersonalData]
    public override string Email { get; set; } = null!;
    public override string PhoneNumber { get; set; } = null!;


    public ICollection<Role> Roles { get; set; } = new List<Role>();


    public void UpdateFromDTO(AppUserDTO dto)
    {
        Id = dto.Id;
        Address = dto.Address;
        Country = dto.Country;
        Email = dto.Email;
        PhoneNumber = dto.PhoneNumber;
    }
}
