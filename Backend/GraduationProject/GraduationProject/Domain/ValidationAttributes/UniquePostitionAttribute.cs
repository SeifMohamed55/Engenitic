namespace GraduationProject.Domain.ValidationAttributes
{
    using GraduationProject.Domain.DTOs;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class UniquePostitionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IEnumerable<IPostitionable> positionable)
            {
                var duplicatePostitions = positionable
                    .GroupBy(q => q.Position)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                .ToList();

                bool allValid = positionable.All(v => v.Position >= 1 && v.Position <= positionable.Count());

                if (!allValid)
                    return new ValidationResult($"Positions must be between 1 and {positionable.Count()}.");

                if (duplicatePostitions.Any())
                    return new ValidationResult($"Duplicate position found: {string.Join(", ", duplicatePostitions)}");
            }

            return ValidationResult.Success;
        }
    }



}
