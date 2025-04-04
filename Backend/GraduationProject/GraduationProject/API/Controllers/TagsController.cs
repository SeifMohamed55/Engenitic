using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class TagsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public TagsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(TagRequest tag)
        {
            if(string.IsNullOrEmpty(tag.Tag))
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "Tag cannot be empty."
                }); 
            try
            {
                var dbTag = _unitOfWork.TagsRepo.AddTag(tag.Tag);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new SuccessResponse()
                {
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "Added Successfully",
                    Data = new TagDTO() { Id = dbTag.Id, Value = dbTag.Value }
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "Tag was not added an error occured."
                });
            }
        }

        [HttpGet("{value}")]
        public async Task<IActionResult> GetTagByValue(string value)
        {
            var dbTag = await _unitOfWork.TagsRepo.GetTagByValueAsync(value);
            if (dbTag == null)
                return NotFound();
            return Ok(new SuccessResponse()
            {
                Code = System.Net.HttpStatusCode.OK,
                Message = "fetched successfully",
                Data = new TagDTO()
                {
                    Value = dbTag.Value,
                    Id = dbTag.Id
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditTag(TagRequest tag)
        {
            try
            {
                var dbTag = await _unitOfWork.TagsRepo.EditTagAsync(tag.Id, tag.Tag);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new SuccessResponse()
                {
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "Added Successfully",
                    Data = new TagDTO() { Id = dbTag.Id, Value = dbTag.Value}
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "Tag was not added an error occured."
                });
            }
        }

    }
}


