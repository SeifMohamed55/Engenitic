using GraduationProject.Domain.DTOs;

namespace GraduationProject.Domain.Models
{
    public class QuizAnswer : IEntity<int> // each question has 1 correct answer
    {
        public QuizAnswer() { }
        public QuizAnswer(AnswerDTO answer)
        {
            Id = answer.Id;
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

        public void UpdateFromDto(AnswerDTO answer)
        {
            AnswerText = answer.AnswerText;
            IsCorrect = answer.IsCorrect;
            Position = answer.Position;
        }
    }



}
