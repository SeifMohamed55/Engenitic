namespace GraduationProject.Models.DTOs
{
    public class UserQuizAttemptDTO
    {
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public List<UserAnswerDTO> UserAnswers { get; set; } = new List<UserAnswerDTO>();
    }
}
