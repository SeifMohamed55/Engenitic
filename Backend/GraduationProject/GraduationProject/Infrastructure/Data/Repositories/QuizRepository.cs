using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;

namespace GraduationProject.Infrastructure.Data.Repositories
{
    public class QuizRepository : BulkRepository<Quiz, int>, IQuizRepository
    {
        private AppDbContext _context { get; set; }

        private static readonly Func<AppDbContext, int, int, Task<QuizDTO?>> GetQuizWithNoAnswerAsync =
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
                    VideoUrl = q.VideoUrl,
                    Description = q.Description,
                    Questions = q.Questions.OrderBy(x => x.Position).Select(qq => new QuestionDTO()
                    {
                        Id = qq.Id,
                        QuestionText = qq.QuestionText,
                        Position = qq.Position,
                        Answers = qq.Answers.OrderBy(x => x.Position).Select(a => new AnswerDTO()
                        {
                            Id = a.Id,
                            AnswerText = a.AnswerText,
                            IsCorrect = false,
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
            return await GetQuizWithNoAnswerAsync(_context, courseId, position);
        }

        public async Task<bool> AddUserQuizAttempt(UserQuizAttemptDTO userQuizAttempt)
        {
            var userQuizAttemptEntity = new UserQuizAttempt()
            {
                QuizId = userQuizAttempt.QuizId,
                UserEnrollmentId = userQuizAttempt.EnrollmentId,
                UserScore = userQuizAttempt.UserAnswers.Select(x=> x.IsCorrect).Count(),
                UserAnswers = userQuizAttempt.UserAnswers.Select(x => new UserAnswer()
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

        public async Task<List<QuizTitleResponse>> GetQuizesTitle(int courseId)
        {
            return await _dbSet
                .Where(x => x.CourseId == courseId)
                .AsNoTracking()
                .Select(x => new QuizTitleResponse()
                {
                    Id = x.Id,
                    Title = x.Title,
                    Position = x.Position,
                    Description = x.Description
                })
                .OrderBy(x=> x.Position)
                .ToListAsync();
        }

    }
}
