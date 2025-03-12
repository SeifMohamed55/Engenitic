using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public interface ITokenRepository : IRepository<RefreshToken>
    {
        void DeleteRefreshToken(int tokenId);
        Task<RefreshToken?> GetUserRefreshToken(int tokenId);

    }

    public class TokenRepository : Repository<RefreshToken>, ITokenRepository
    {
        public TokenRepository(AppDbContext context) : base(context)
        {

        }

        public void DeleteRefreshToken(int tokenId)
        {
            Delete(tokenId);
        }

        public async Task<RefreshToken?> GetUserRefreshToken(int tokenId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == tokenId);
        }

    }
}
