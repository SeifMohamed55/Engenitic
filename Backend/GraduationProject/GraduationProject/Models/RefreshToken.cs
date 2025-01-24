using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace GraduationProject.Models;

public class RefreshToken 
{
    public int Id { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public string EncryptedToken { get; set; } = null!;
    public virtual string LoginProvider { get; set; } = default!;

    public AppUser AppUser { get; set; } = null!;

}