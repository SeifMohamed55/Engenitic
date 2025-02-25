using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers.ApiRequest
{
    public class ConfirmEmailRequest
    {
        public string UserId { get; set; } = null!;
        public string NewEmail { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
