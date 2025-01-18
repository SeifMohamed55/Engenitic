using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace GraduationProject.Models;

public partial class RefreshToken 
{

    public DateTimeOffset ExpiryDate { get; set; }
    public string Value { get; set; } = null!;
    public virtual string LoginProvider { get; set; } = default!;

    public string Name { get; set; } = null!;

}