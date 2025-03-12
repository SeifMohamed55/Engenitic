using System.ComponentModel.DataAnnotations;


public class NotEmptyCollectionAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IEnumerable<object> enumerable && !enumerable.Any())
        {
            return new ValidationResult($"{validationContext.DisplayName} cannot be empty.");
        }
        return ValidationResult.Success;
    }
}
