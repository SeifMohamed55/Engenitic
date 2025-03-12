using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class UpdateUsernameRequest
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(120)]
        public string NewUsername { get; set; } = null!;
    }
}
