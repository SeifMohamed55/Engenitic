using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class UpdateUsernameRequest
    {
        [Required]
        [MaxLength(120)]
        public string NewUsername { get; set; } = null!;
    }
}
