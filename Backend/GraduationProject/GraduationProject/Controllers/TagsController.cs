using GraduationProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="admin")]
    public class TagsController : ControllerBase
    {
        private readonly ITagsRepository _tagsRepo;
        public TagsController(ITagsRepository tagsRepository) 
        {
            _tagsRepo = tagsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTags() 
        {
            var tags = await _tagsRepo.GetAllAsync();
            return Ok(tags);
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(string tag)
        {
            var dbTag = await _tagsRepo.AddTagAsync(tag);
            return Ok(dbTag);
        }

        [HttpGet("{value}")]
        public async Task<IActionResult> GetTagByValue(string value)
        {
            var dbTag = await _tagsRepo.GetTagByValueAsync(value);
            if (dbTag == null)
                return NotFound();
            return Ok(dbTag);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditTag(int id, string newValue)
        {
            var dbTag = await _tagsRepo.EditTagAsync(id, newValue);
            return Ok(dbTag);
        }

    }
}


