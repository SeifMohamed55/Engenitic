namespace GraduationProject.Domain.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public string EncryptedToken { get; set; } = null!;
    public virtual string LoginProvider { get; set; } = default!;
    public string LatestJwtAccessTokenJti { get; set; } = null!;
    public DateTimeOffset LatestJwtAccessTokenExpiry { get; set; }
    public AppUser AppUser { get; set; } = null!;

}