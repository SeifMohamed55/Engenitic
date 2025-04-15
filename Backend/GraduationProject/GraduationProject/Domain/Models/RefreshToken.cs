using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;

namespace GraduationProject.Domain.Models;

public class RefreshToken
{
    public required Guid DeviceId { get; set; }
    public required Guid Token { get; set; }
    public required string IpAddress { get; set; } = null!;
    public required string UserAgent { get; set; } = null!;
    public DateTimeOffset IssuedAt { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public required string LoginProvider { get; set; } = null!;
    public string LatestJwtAccessTokenJti { get; set; } = null!;
    public DateTimeOffset LatestJwtAccessTokenExpiry { get; set; }

    public int UserId{ get; set; }
    public AppUser AppUser { get; set; } = null!;

}