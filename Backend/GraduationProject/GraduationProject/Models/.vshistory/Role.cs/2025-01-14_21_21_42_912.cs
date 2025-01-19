
using Microsoft.AspNetCore.Identity;
namespace GraduationProject.Models;

public class Role : IdentityRole<int>
{
   
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}