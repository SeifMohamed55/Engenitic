using GraduationProject.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Repositories
{

    public interface IQuizRepository
    {
        Task<QuizDTO?> GetQuizByCourseAndPosition(int courseId, int position);
       // Task<bool> AddUserQuizAttempt(UserQuizAttemptDTO userQuizAttempt); // TODO: Implement this method
    }
    public class QuizRepository : IQuizRepository
    {
        private AppDbContext _context { get; set; }

        private static readonly Func<AppDbContext, int, int, Task<QuizDTO?>>GetQuizAsync =
        EF.CompileAsyncQuery((AppDbContext dbContext, int courseId, int position) =>
            dbContext.Quizzes
                .Include(x => x.Questions)
                    .ThenInclude(q => q.Answers)
                .Where(q => q.CourseId == 1 && q.Position == 1)
                .Select(q => new QuizDTO()
                {
                    Id = q.Id,
                    Title = q.Title,
                    Position = q.Position,
                    Questions = q.Questions.OrderBy(x => x.Position).Select(qq => new QuestionDTO()
                    {
                        Id = qq.Id,
                        QuestionText = qq.QuestionText,
                        Position = qq.Position,
                        Answers = qq.Answers.OrderBy(x => x.Position).Select(a => new AnswerDTO()
                        {
                            Id = a.Id,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            Position = a.Position
                        }).ToList()
                    }).ToList()
                })
                .AsSingleQuery()
                .FirstOrDefault());




        public QuizRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuizDTO?> GetQuizByCourseAndPosition(int courseId, int position)
        {
            return await GetQuizAsync(_context, courseId, position);
        }

/*        public Task<bool> AddUserQuizAttempt(UserQuizAttemptDTO userQuizAttempt)
        {
            var ans = _answers.Include(x => x.Question)
                .Where(x => userQuizAttempt.UserAnswers.Select(x => x.AnswerId).Contains(x.Id))

        }*/
    }
}
