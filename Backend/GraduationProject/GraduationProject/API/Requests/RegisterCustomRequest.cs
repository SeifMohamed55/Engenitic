using GraduationProject.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class RegisterCustomRequest : IValidatableObject
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
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;


        [Phone]
        public string? PhoneNumber { get; set; }

        [RegularExpression(@"^\+\d{1,4}$", ErrorMessage = "The country code must start with a '+' followed by 1 to 4 digits.")]
        public string? PhoneRegion { get; set; }

        public IFormFile? Image { get; set; }

        public IFormFile? Cv { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Role?.Equals(Roles.Instructor, StringComparison.OrdinalIgnoreCase) == true && Cv == null)
            {
                yield return new ValidationResult(
                    "CV is required for Instructors.",
                    new[] { nameof(Cv) });
            }
        }
    }
}
