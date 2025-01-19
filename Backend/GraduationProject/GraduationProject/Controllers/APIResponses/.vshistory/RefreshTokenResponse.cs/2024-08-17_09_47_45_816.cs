using System.ComponentModel.DataAnnotations;

namespace SpringBootCloneApp.Controllers.ResponseModels
{
    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = null!;

        public string ValidTo { get; set; } = null!;
    }
}
