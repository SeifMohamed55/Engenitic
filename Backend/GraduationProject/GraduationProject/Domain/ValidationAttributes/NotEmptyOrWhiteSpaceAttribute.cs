namespace GraduationProject.Domain.ValidationAttributes
{
    using System.ComponentModel.DataAnnotations;

    public class NotEmptyOrWhiteSpaceAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && string.IsNullOrWhiteSpace(str)) // Checks null, empty, or only spaces
            {
                return new ValidationResult("The field cannot be empty or whitespace.");
            }
            return ValidationResult.Success;
        }
    }

}
