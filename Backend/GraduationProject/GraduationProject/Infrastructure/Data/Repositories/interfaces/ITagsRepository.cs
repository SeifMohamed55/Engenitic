using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface ITagsRepository : IBulkRepository<Tag, int>, ICustomRepository
    {
        Tag AddTag(string tag);
        Task<Tag?> GetTagByValueAsync(string value);
        Task<Tag> EditTagAsync(int id, string newValue);
        Task<List<TagDTO>> GetTagsDTOAsync();

    }
}
