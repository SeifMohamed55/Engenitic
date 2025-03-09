using GraduationProject.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models.DTOs
{
    public class QuizDTO : IPostitionable
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [NotEmptyOrWhiteSpace]
        public string Title { get; set; } = null!;
        public int Position { get; set; }
        [Required]
        [NotEmptyOrWhiteSpace]
        public string VideoUrl { get; set; } = null!;

        [Required]
        [UniquePostition]
        [NotEmptyCollection]
        public List<QuestionDTO> Questions { get; set; } = new List<QuestionDTO>();

       
    }
}
