using GraduationProject.Application.Services;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public interface IFileHashRepository : IBulkRepository<FileHash, int>
    {
        Task<FileHash> GetDefaultUserImageAsync();
        Task<FileHash> GetDefaultCourseImageAsync();
        bool IsDefaultCourseImageHash(FileHash hash);
        bool IsDefaultUserImageHash(FileHash hash);
    }
    public class FileHashRepository : BulkRepository<FileHash, int>, IFileHashRepository
    {
        public FileHashRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<FileHash> GetDefaultUserImageAsync()
        {
            return await _dbSet
                .FirstAsync(x => x.PublicId == ICloudinaryService.DefaultUserImagePublicId);
        }


        public async Task<FileHash> GetDefaultCourseImageAsync()
        {
            return await _dbSet
                .FirstAsync(x => x.PublicId == ICloudinaryService.DefaultCourseImagePublicId);
        }

        public bool IsDefaultCourseImageHash(FileHash hash)
        {
            return hash.PublicId == ICloudinaryService.DefaultCourseImagePublicId;
        }

        public bool IsDefaultUserImageHash(FileHash hash)
        {
            return hash.PublicId == ICloudinaryService.DefaultUserImagePublicId;
        }
    }
}
