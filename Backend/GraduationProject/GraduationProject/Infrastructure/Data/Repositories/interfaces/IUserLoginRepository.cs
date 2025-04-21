using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface IUserLoginRepository : IGenericRepository<IdentityUserLogin<int>>, ICustomRepository
    {
        Task<bool> ContainsLoginProvider(int userId, string provider);
    }
}
