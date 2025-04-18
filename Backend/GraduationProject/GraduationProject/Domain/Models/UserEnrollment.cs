namespace GraduationProject.Domain.Models
{
    public class UserEnrollment : IEntity<int>
    {
        public int Id { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public int CurrentStage { get; set; }
        public bool IsCompleted { get; set; }

        public int TotalStages { get; init; }

        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<UserQuizAttempt> QuizAttempts { get; set; } = new List<UserQuizAttempt>();



    }
}
