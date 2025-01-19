using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models.DTOs
{
    public class AppUserDTO
    {
        [Required]
        public int id { get; set; }
        public string Address { get; set; } = null!;

        public string Country { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;


        public ICollection<string> Roles { get; set; } = new List<string>();
    }
}
