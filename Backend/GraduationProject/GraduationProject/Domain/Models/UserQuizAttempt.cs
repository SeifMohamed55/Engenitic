namespace GraduationProject.Domain.Models
{
    public class UserQuizAttempt
    {
        public int Id { get; set; }
        public int UserScore { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public int UserEnrollmentId { get; set; }
        public UserEnrollment UserEnrollment { get; set; } = null!;

        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}
