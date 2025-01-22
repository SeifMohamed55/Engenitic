using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace GraduationProject.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }

    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        protected readonly DbSet<T> _appUsers;

        public Repository(AppDbContext context)
        {
            _context = context;
            _appUsers = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _appUsers.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _appUsers.FindAsync(id);
        }

        public async Task<bool> Exists(int id)
        {
            if ((await GetByIdAsync(id)) != null)
                return true;
            return false;
        }

        public async Task AddAsync(T entity)
        {
            await _appUsers.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _appUsers.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    _appUsers.Remove(entity);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch(Exception)
            {
            }
            return false;
        }

    }


}
