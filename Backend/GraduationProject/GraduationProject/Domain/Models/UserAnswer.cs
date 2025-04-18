namespace GraduationProject.Domain.Models
{

    public class UserAnswer : IEntity<int>
    {
        public int Id { get; set; }
        public bool IsCorrect { get; set; }
        public int UserQuizAttemptId { get; set; }

        public int? QuestionId { get; set; }
        public QuizQuestion? Question { get; set; } = null!;

        public int? AnswerId { get; set; }
        public QuizAnswer? Answer { get; set; } = null!;

    }
}
