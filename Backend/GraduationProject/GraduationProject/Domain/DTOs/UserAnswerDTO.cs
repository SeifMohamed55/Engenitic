namespace GraduationProject.Domain.DTOs
{
    public class UserAnswerDTO
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }
}