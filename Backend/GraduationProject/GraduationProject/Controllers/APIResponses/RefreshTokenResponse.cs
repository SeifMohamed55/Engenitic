using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Controllers.APIResponses
{
    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = null!;

        public string ValidTo { get; set; } = null!;
    }
}
