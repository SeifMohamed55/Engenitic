using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;

namespace GraduationProject.Common.Extensions
{
    public static class QuizQuestionExtension
    {
        public static IQueryable<UserAnswerDTO> QuestionWithAnswer(this IQueryable<QuizQuestion> query)
        {
            return query
                .Select(q => new UserAnswerDTO()
                {
                    QuestionId = q.Id,
                    AnswerId = q.Answers.Where(x => x.IsCorrect).Select(x => x.Id).FirstOrDefault(),
                    IsCorrect = true
                });
        }
    }
}
