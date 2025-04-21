namespace GraduationProject.Infrastructure.Data.Interfaces
{
    using GraduationProject.Domain.Models;
    using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;
    using GraduationProject.Infrastructure.Data.Repositories.interfaces;

    public interface IUnitOfWork
    {
        public IUserRepository UserRepo { get; }
        public ITokenRepository TokenRepo { get; }
        public ICourseRepository CourseRepo { get; }
        public IEnrollmentRepository EnrollmentRepo { get; }
        public IQuizRepository QuizRepo { get; }
        public ITagsRepository TagsRepo { get; }
        public IUserLoginRepository UserLoginRepo { get; }
        public IFileHashRepository FileHashRepo { get; }
        public IQuizQuestionRepository QuizQuestionRepo { get; set; }
        public IBulkRepository<QuizAnswer, int> QuizAnswerRepo { get; set; }
        public IReviewRepository ReviewRepository { get; set; }

        Task SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}
