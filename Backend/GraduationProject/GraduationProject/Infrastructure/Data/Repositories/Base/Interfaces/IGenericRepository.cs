namespace GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces
{
    public interface IGenericRepository<T> : IRepository where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        void Insert(T entity);
        void Update(T entity);
        void Delete(object id);
        void Delete(T entityToDelete);
        void Detach(T entity);
    }


}
