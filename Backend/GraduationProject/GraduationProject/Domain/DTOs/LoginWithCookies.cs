using GraduationProject.API.Responses;
using GraduationProject.Domain.Models;

namespace GraduationProject.Domain.DTOs
{
    public class LoginWithCookies
    {
        public LoginResponse LoginResponse { get; set; } = null!;
        public RefreshToken RefreshToken { get; set; } = null!;
    }
}
