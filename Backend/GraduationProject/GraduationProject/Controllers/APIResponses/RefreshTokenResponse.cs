using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Controllers.APIResponses
{
    public class AccessTokenResponse
    {
        public string ValidTo { get; set; } = null!;
        public string AccessToken { get; set; } = null!;

    }
}
