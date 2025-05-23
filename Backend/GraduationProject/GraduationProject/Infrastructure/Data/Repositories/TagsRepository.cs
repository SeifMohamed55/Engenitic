﻿using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public class TagsRepository : BulkRepository<Tag, int>, ITagsRepository
    {
        public TagsRepository(AppDbContext context) : base(context)
        {
        }

        // Add Tag
        public Tag AddTag(string tag)
        {
            var dbTag = new Tag(tag);
            Insert(dbTag);
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
            var tag = await GetByIdAsync(id);
            if (tag == null)
                throw new Exception("Tag not found");
            tag.Value = newValue;
            Update(tag);
            return tag;
        }

        // Get All Tags
        public async Task<List<Tag>> GetAllAsync(int id)
        {
            return await GetAllAsync();
        }

        public async Task<List<TagDTO>> GetTagsDTOAsync()
        {
            return await _dbSet
                .Include(x => x.Courses)
                .Select(x => new TagDTO
                {
                    Id = x.Id,
                    Value = x.Value,
                    Count = x.Courses.Count
                }).ToListAsync();    
        }
    }
}
