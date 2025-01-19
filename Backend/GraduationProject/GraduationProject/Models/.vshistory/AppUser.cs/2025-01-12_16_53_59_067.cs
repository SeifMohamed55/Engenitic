
using Microsoft.AspNetCore.Identity;

internal class AppUser : IdentityUser<int>
{
    public string Address { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    [ProtectedPersonalData]
    public virtual string? Email { get; set; }
    public override string PhoneNumber { get; set; } = null!;
}