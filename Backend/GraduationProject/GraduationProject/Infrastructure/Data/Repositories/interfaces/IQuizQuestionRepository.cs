using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface IQuizQuestionRepository : IBulkRepository<QuizQuestion, int>, ICustomRepository
    {
        Task<Dictionary<int, UserAnswerDTO>> GetQuizWithQuestionsByIdAsync(int quizId);
    }
}
