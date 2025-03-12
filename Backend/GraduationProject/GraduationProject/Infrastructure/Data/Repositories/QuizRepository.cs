using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public interface IQuizRepository : IRepository<Quiz>
    {
        Task<QuizDTO?> GetQuizByCourseAndPosition(int courseId, int position);
        Task<bool> AddUserQuizAttempt(UserQuizAttemptDTO userQuizAttempt); // TODO: Implement this method
    }
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        private AppDbContext _context { get; set; }

        private static readonly Func<AppDbContext, int, int, Task<QuizDTO?>> GetQuizAsync =
        EF.CompileAsyncQuery((AppDbContext dbContext, int courseId, int position) =>
            dbContext.Quizzes
                .Include(x => x.Questions)
                    .ThenInclude(q => q.Answers)
                .Where(q => q.CourseId == courseId && q.Position == position)
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




        public QuizRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<QuizDTO?> GetQuizByCourseAndPosition(int courseId, int position)
        {
            return await GetQuizAsync(_context, courseId, position);
        }

        public async Task<bool> AddUserQuizAttempt(UserQuizAttemptDTO userQuizAttempt)
        {
            var userAnswers = await _context.QuizAnswers
                .Include(x => x.Question)
                .Where(x => userQuizAttempt.UserAnswers.Select(x => x.AnswerId).Contains(x.Id))
                .Join(userQuizAttempt.UserAnswers,
                    quizAnswer => quizAnswer.Id,
                    userAnswer => userAnswer.AnswerId,
                    (quizAnswer, userAnswer) => new UserAnswerDTO
                    {
                        AnswerId = userAnswer.AnswerId,
                        QuestionId = quizAnswer.QuestionId,
                        IsCorrect = quizAnswer.IsCorrect
                    })
                .ToListAsync();

            var userQuizAttemptEntity = new UserQuizAttempt()
            {
                QuizId = userQuizAttempt.QuizId,
                UserEnrollmentId = userQuizAttempt.EnrollmentId,
                UserScore = userAnswers.Count(x => x.IsCorrect),
                UserAnswers = userAnswers.Select(x => new UserAnswer()
                {
                    AnswerId = x.AnswerId,
                    QuestionId = x.QuestionId,
                    IsCorrect = x.IsCorrect,
                }).ToList()
            };

            try
            {
                _context.UserQuizAttempts.Add(userQuizAttemptEntity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
