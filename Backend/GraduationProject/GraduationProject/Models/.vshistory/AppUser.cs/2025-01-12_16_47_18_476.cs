
using Microsoft.AspNetCore.Identity;

internal class AppUser : IdentityUser
{
    public string Username { get; set; } = null!;
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string PhoneNumber { get; set; }
    public
}