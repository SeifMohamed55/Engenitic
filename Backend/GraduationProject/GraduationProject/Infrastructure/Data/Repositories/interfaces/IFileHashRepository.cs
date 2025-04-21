using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface IFileHashRepository : IBulkRepository<FileHash, int>, ICustomRepository
    {
        Task<FileHash> GetDefaultUserImageAsync();
        Task<FileHash> GetDefaultCourseImageAsync();
        bool IsDefaultCourseImageHash(FileHash hash);
        bool IsDefaultUserImageHash(FileHash hash);
    }
}
