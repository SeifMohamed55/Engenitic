
using Microsoft.AspNetCore.Identity;
namespace GraduationProject.Models;

public class Role : IdentityRole<int>
{
    public override string Name { get; set; } = null!;
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}