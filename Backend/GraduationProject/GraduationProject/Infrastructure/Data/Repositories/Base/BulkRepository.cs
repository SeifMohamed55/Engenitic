using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories.Base
{

    public class BulkRepository<T, TKey> : Repository<T>, IBulkRepository<T, TKey> where T : class, IEntity<TKey>
    {
        public BulkRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<int> ExecuteDeleteAsync(IReadOnlySet<TKey> ids)
        {
            return await _dbSet.Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync();
        }

        public async Task AddRangeAsync(IEnumerable<T> quizzes)
        {
            await _dbSet.AddRangeAsync(quizzes);
        }

        public void RemoveRange(IEnumerable<T> quizzes)
        {
            _dbSet.RemoveRange(quizzes);
        }
    }
}
