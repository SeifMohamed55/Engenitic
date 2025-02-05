namespace GraduationProject.Models
{
    public class QuizAnswer // each question has 1 answer
    {
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; } = null!;
    }



}
