using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using System.Linq.Expressions;

namespace GraduationProject.Infrastructure.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        void Insert(T entity);
        void Update(T entity);
        void Delete(object id);
        void Delete(T entityToDelete);
        void Detach(T entity);
    }


    public class Repository<T> : IRepository<T>  where T : class
    {
        private readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
      
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public void Insert(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Delete(object id)
        {
            T? entityToDelete = _dbSet.Find(id);
            if (entityToDelete == null)
                throw new ArgumentNullException("Token does not exist");

            Delete(entityToDelete);
        }

        public virtual void Delete(T entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public void Detach(T entity)
        {
            _dbSet.Entry(entity).State = EntityState.Detached;
        }
    }


}
