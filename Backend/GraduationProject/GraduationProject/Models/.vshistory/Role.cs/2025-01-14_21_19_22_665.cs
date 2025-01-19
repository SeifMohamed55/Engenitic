
using Microsoft.AspNetCore.Identity;

public class Role : IdentityRole<int>
{
   
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}