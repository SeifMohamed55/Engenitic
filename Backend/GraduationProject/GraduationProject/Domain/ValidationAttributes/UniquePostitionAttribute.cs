namespace GraduationProject.Domain.ValidationAttributes
{
    using GraduationProject.Domain.DTOs;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class UniquePostitionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection<IPostitionable> positionable)
            {
                var duplicatePostitions = positionable
                    .GroupBy(q => q.Position)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicatePostitions.Any())
                    return new ValidationResult($"Duplicate position found: {string.Join(", ", duplicatePostitions)}");

                if (positionable.Any(p => p.Position == 0))
                    return new ValidationResult("Cannot set position with 0.");
            }

            return ValidationResult.Success;
        }
    }



}
