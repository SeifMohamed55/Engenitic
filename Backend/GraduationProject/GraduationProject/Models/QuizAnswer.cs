using GraduationProject.Models.DTOs;

namespace GraduationProject.Models
{
    public class QuizAnswer // each question has 1 answer
    {
        public QuizAnswer() { }
        public QuizAnswer(AnswerDTO answer)
        {
            AnswerText = answer.AnswerText;
            IsCorrect = answer.IsCorrect;
            Position = answer.Position;
        }
        public int Id { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Position { get; set; }

        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; } = null!;
    }



}
