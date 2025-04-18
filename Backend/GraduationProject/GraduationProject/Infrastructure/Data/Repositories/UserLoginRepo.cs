using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public interface IUserLoginRepository : IRepository<IdentityUserLogin<int>>
    {
        Task<bool> ContainsLoginProvider(int userId, string provider);
    }

    public class UserLoginRepository : Repository<IdentityUserLogin<int>>, IUserLoginRepository
    {

        public UserLoginRepository(AppDbContext context) : base(context) { }

        public async Task<bool> ContainsLoginProvider(int userId, string provider)
        {
            return await _dbSet.Where(x => x.UserId == userId)
                .Select(x => x.LoginProvider)
                .ContainsAsync(provider);
        }
    }
}
