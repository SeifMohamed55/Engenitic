namespace GraduationProject.Infrastructure.Data
{
    using GraduationProject.Infrastructure.Data.Repositories;
    using Microsoft.EntityFrameworkCore.Storage;
    using System;

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

        Task SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction = null;


        public IUserRepository UserRepo { get; }
        public ITokenRepository TokenRepo { get; }
        public ICourseRepository CourseRepo { get; }
        public IEnrollmentRepository EnrollmentRepo { get; }
        public IQuizRepository QuizRepo { get; }
        public ITagsRepository TagsRepo { get; }
        public IUserLoginRepository UserLoginRepo { get; }
        public IFileHashRepository FileHashRepo { get; }


        public UnitOfWork
            (
            AppDbContext context,
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IQuizRepository quizRepository,
            ITagsRepository tagsRepository,
            IUserLoginRepository userLoginRepository,
            IFileHashRepository fileHashRepository
            )
        {
            _context = context;
            UserRepo = userRepository;
            TokenRepo = tokenRepository;
            CourseRepo = courseRepository;
            EnrollmentRepo = enrollmentRepository;
            QuizRepo = quizRepository;
            TagsRepo = tagsRepository;
            UserLoginRepo = userLoginRepository;
            FileHashRepo = fileHashRepository;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction is available.");

            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();

            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

    }
}
