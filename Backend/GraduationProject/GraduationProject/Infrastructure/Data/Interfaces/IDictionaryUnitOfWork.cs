using GraduationProject.Infrastructure.Data.Repositories.interfaces;

namespace GraduationProject.Infrastructure.Data.Interfaces
{
    public interface IDictionaryUnitOfWork
    {
        TRepo GetRepository<TRepo>() where TRepo : class, ICustomRepository;
        Task SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
