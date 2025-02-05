namespace GraduationProject.Models
{
    /*    public class UserQuizAttempt // add enrollment
        {
            public int Id { get; set; }
            public int UserId { get; set; }  
            public int UserScore { get; set; }
            public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

            public int QuizId { get; set; }
            public Quiz Quiz { get; set; } = null!;

            public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        }*/

    public class UserAnswer
    {
        public int Id { get; set; }
        public bool IsCorrect { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; } = null!;

        public int AnswerId { get; set; }
        public QuizAnswer Answer { get; set; } = null!;
    }



}
