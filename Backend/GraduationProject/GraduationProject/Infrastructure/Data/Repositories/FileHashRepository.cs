using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{
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
