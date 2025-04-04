using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GraduationProject.API.Controllers
{
    // TODO: Tags

    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesService _coursesService;

        public CoursesController(ICoursesService coursesService)
        {
            _coursesService = coursesService;
        }


        [HttpGet("{index}")]
        public async Task<IActionResult> GetPageOfCourses(int index = 1)
        {
            if (index <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesService.GetPageOfCourses(index);

                if (index > courses.TotalPages)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid Page Number",
                        Code = HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse()
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<CourseDTO>(courses),
                    Code = HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.InternalServerError,
                });
            }
        }


        [HttpGet("search")]
        public async Task<IActionResult> GetCoursesBySearching
                                        ([FromQuery] string search, [FromQuery] int index = 1)
        {
            if (index <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesService.SearchOnPageOfCourses(search, index);

                if (index > courses.TotalPages)
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid Page Number",
                        Code = HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<CourseDTO>(courses),
                    Code = HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.InternalServerError,
                });

            }
        }


        [HttpGet("search/tag")]
        public async Task<IActionResult> GetCoursesByTag
                                ([FromQuery] string tag, [FromQuery] int index = 1)
        {
            if (index <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesService.GetPageOfCoursesByTag(tag, index);

                if (index > courses.TotalPages)
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid Page Number",
                        Code = HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<CourseDTO>(courses),
                    Code = HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.InternalServerError,
                });

            }
        }


        [HttpGet("tags")]
        public async Task<IActionResult> GetTags()
        {
            try
            {
                var tags = await _coursesService.GetAllTagsAsync();

                return Ok(new SuccessResponse()
                {
                    Message = "Tags Retrieved Successfully.",
                    Data = tags,
                    Code = HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.InternalServerError,
                });
            }
        }

        [HttpGet("id/{courseId}")]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            try
            {
                var course = await _coursesService.GetCourseDetailsById(courseId);

                if (course == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Course Not Found.",
                        Code = HttpStatusCode.NotFound,
                    });

                return Ok(new SuccessResponse()
                {
                    Message = "Course Retrieved Successfully.",
                    Data = course,
                    Code = HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.InternalServerError,
                });
            }
        }


        /*[HttpPost("dummyHashImage")]
        public async Task<IActionResult> HashDefaults()
        {
            var courseHash = await _unitOfWork.FileHashRepo.FirstOrDefaultAsync(x=> x.PublicId == _cloudinary.DefaultCourseImagePublicId);
            var userHash = await _unitOfWork.FileHashRepo.FirstOrDefaultAsync(x=> x.PublicId == _cloudinary.DefaultUserImagePublicId);

            if (courseHash == null || userHash == null)
                return BadRequest();

            var courses = await _unitOfWork.CourseRepo.GetAllCoursesAsync();
            var users = await _unitOfWork.UserRepo.GetAllUsersAsync();

            foreach (var course in courses)
            {
                course.FileHash = courseHash;
            }
            foreach(var user in users)
            {
                user.FileHashes.Add(userHash);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }*/

    }
}
