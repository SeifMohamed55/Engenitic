﻿
using Microsoft.AspNetCore.Identity;

internal class AppUser : IdentityUser<int>
{
    public string Address { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    [ProtectedPersonalData]
    public override string Email { get; set; } = null!;
    public override string PhoneNumber { get; set; } = null!;
}