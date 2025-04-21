using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface IQuizRepository : IBulkRepository<Quiz, int>, ICustomRepository
    {
        Task<QuizDTO?> GetQuizByCourseAndPosition(int courseId, int position);
        Task<bool> AddUserQuizAttempt(UserQuizAttemptDTO userQuizAttempt); // TODO: Implement this method
        Task<List<QuizTitleResponse>> GetQuizesTitle(int courseId);
    }
}
