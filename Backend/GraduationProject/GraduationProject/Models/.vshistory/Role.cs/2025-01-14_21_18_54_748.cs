
using Microsoft.AspNetCore.Identity;

public class Role : IdentityRole<int>
{
   
    public ICollection<AppUser> User { get; set; } = new List<AppUser>();
}