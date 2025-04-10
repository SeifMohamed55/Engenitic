using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Domain.DTOs
{
    public class UserAnswerDTO
    {
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public int AnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }
}