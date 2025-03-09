using GraduationProject.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models.DTOs
{
    public class QuestionDTO : IPostitionable, IValidatableObject
    {
        public int Id { get; set; }
        [Required]
        [StringLength(600)]
        [NotEmptyOrWhiteSpace]
        public string QuestionText { get; set; } = null!;

        public int Position { get; set; }


        [Required]
        [UniquePostition]
        [NotEmptyCollection]
        [MinLength(4)]
        public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Answers.Count(x=> x.IsCorrect) != 1) 
            {
                yield return new ValidationResult("Must have one correct answer.", [nameof(Answers)]);
            }
        }
    }
}
