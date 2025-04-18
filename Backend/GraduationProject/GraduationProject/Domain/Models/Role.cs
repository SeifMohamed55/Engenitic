using Microsoft.AspNetCore.Identity;
namespace GraduationProject.Domain.Models;

// CamelCase naming convention is used for the Name attribute
public class Role : IdentityRole<int>, IEntity<int>
{
    public override string Name { get; set; } = null!;
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();

}