using GraduationProject.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models.DTOs
{
    public class QuestionDTO : IPostitionable
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
        public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();

    }
}
