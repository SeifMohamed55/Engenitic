using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class ForgetPasswordRequest
    {
        [Required]
        public string Email { get; set; } = null!;

    }
}
