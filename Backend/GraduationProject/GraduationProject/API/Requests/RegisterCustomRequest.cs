﻿using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class RegisterCustomRequest
    {
        [Required]
        [MaxLength(120)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(5)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(5)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;


        [Phone]
        public string? PhoneNumber { get; set; }

        [RegularExpression(@"^\+\d{1,4}$", ErrorMessage = "The country code must start with a '+' followed by 1 to 4 digits.")]
        public string? PhoneRegion { get; set; }

        public IFormFile? Image { get; set; }
    }
}
