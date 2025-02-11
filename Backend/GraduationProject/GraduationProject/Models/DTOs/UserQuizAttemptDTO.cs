namespace GraduationProject.Models.DTOs
{
    public class UserQuizAttemptDTO
    {
        public int EnrollmentId { get; set; }
        public int QuizId { get; set; }
        public List<UserAnswerDTO> UserAnswers { get; set; } = new List<UserAnswerDTO>();
    }
}
