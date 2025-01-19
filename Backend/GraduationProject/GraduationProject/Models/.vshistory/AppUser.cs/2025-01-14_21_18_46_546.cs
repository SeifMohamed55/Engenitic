
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


    public ICollection<Role> UserRoles { get; set; } = new List<Role>();
}


public class Role : IdentityRole<int>
{
   
    public ICollection<AppUser> User { get; set; } = new List<AppUser>();
}