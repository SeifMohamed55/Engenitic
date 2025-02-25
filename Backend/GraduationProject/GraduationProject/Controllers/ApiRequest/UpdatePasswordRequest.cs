using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Controllers.ApiRequest
{
    public class UpdatePasswordRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(5)]
        public string OldPassword { get; set; } = null!;

        [Required]
        [MinLength(5)]
        public string NewPassword { get; set; } = null!;
    }
}
