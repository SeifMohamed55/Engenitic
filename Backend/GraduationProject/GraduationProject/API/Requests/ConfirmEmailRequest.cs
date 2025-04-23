using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class ConfirmEmailRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string NewEmail { get; set; } = null!;
        [Required]
        public string Token { get; set; } = null!;
    }

}
