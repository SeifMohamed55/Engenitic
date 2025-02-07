using Microsoft.AspNetCore.Identity;
using NuGet.Configuration;
namespace GraduationProject.Models;

// CamelCase naming convention is used for the Name attribute
public class Role : IdentityRole<int>
{
    public override string Name { get; set; } = null!;
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();

}