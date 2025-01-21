using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models.DTOs
{
    public class AppUserDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; } = null!;

        public string? imageURL { get; set; }

        public string? RegionCode { get; set; }

    }
}
