using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class UpdatePasswordRequest
    {

        [Required]
        [MinLength(5)]
        public string OldPassword { get; set; } = null!;

        [Required]
        [MinLength(5)]
        public string NewPassword { get; set; } = null!;
    }
}
