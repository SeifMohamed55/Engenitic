using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{
    public interface IQuizQuestionRepository : IRepository<QuizQuestion>
    {
        Task<Dictionary<int, UserAnswerDTO>> GetQuizWithQuestionsByIdAsync(int quizId);
    }

    public class QuizQuestionRepository : Repository<QuizQuestion>, IQuizQuestionRepository
    {
        public QuizQuestionRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<Dictionary<int, UserAnswerDTO>> GetQuizWithQuestionsByIdAsync(int quizId)
        {
            return await _dbSet
                .Include(q => q.Answers)
                .Where(x => x.QuizId == quizId)
                .Select(q => new UserAnswerDTO()
                {
                    QuestionId = q.Id,
                    AnswerId = q.Answers.Where(x => x.IsCorrect).Select(x=> x.Id).FirstOrDefault(),
                })
                .AsSingleQuery()
                .ToDictionaryAsync((x) => x.QuestionId);

        }
    }
}
