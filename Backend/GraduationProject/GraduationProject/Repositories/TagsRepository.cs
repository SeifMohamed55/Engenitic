using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Repositories
{
    public interface ITagsRepository 
    {
        Task<Tag> AddTagAsync(string tag);
        Task<Tag?> GetTagByValueAsync(string value);
        Task<Tag> EditTagAsync(int id, string newValue);
        Task<List<Tag>> GetAllAsync();

    }

    public class TagsRepository : Repository<Tag>, ITagsRepository  
    {
        public TagsRepository(AppDbContext context) : base(context)
        {
        }

        // Add Tag
        public async Task<Tag> AddTagAsync(string tag)
        {
            var dbTag = new Tag(tag);
            await this.AddAsync(dbTag);
            return dbTag;
        }

        // Get Tag by Value
        public async Task<Tag?> GetTagByValueAsync(string value)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Value == value);
        }

        // Edit Tag Name
        public async Task<Tag> EditTagAsync(int id, string newValue)
        {
            var tag = await this.GetByIdAsync(id);
            if (tag == null)
                throw new Exception("Tag not found");
            tag.Value = newValue;
            await UpdateAsync(tag);
            return tag;
        }

        // Get All Tags
        public async Task<List<Tag>> GetAllAsync(int id)
        {
            return await GetAllAsync();
        }


    }
}
