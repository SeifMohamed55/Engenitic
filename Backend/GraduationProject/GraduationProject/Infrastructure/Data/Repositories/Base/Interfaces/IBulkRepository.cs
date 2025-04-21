using GraduationProject.Domain.Models;

namespace GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces
{
    public interface IBulkRepository<T, TKey> : IGenericRepository<T> where T : class, IEntity<TKey>
    {
        Task<int> ExecuteDeleteAsync(IReadOnlySet<TKey> ids);
        Task AddRangeAsync(IEnumerable<T> quizzes);
        void RemoveRange(IEnumerable<T> quizzes);
    }
}
