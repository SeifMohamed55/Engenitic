using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GraduationProject.Infrastructure.Data
{

    // Under testing
    public class DictionaryUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction = null;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _repositories;

        public IUserRepository UserRepo => GetRepository<IUserRepository>();

        public ITokenRepository TokenRepo => GetRepository<ITokenRepository>();

        public ICourseRepository CourseRepo => GetRepository<ICourseRepository>();

        public IEnrollmentRepository EnrollmentRepo => GetRepository<IEnrollmentRepository>();

        public IQuizRepository QuizRepo => GetRepository<IQuizRepository>();

        public ITagsRepository TagsRepo => GetRepository<ITagsRepository>();

        public IUserLoginRepository UserLoginRepo => GetRepository<IUserLoginRepository>();

        public IFileHashRepository FileHashRepo => GetRepository<IFileHashRepository>();

        public IQuizQuestionRepository QuizQuestionRepo  => GetRepository<IQuizQuestionRepository>();
        public IBulkRepository<QuizAnswer, int> QuizAnswerRepo => GetRepository<IBulkRepository<QuizAnswer, int>>();
        public IReviewRepository ReviewRepository => GetRepository<IReviewRepository>();

        public DictionaryUnitOfWork(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _repositories = new Dictionary<Type, object>();
        }

        // with no properties TODO: add ICustomRepository in constrains
        private TRepo GetRepository<TRepo>() where TRepo : class, IRepository 
        {
            var type = typeof(TRepo);
            if (!_repositories.TryGetValue(type, out var repo))
            {
                repo = _serviceProvider.GetRequiredService<TRepo>();
                _repositories[type] = repo;
            }
            return (TRepo)repo;
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
