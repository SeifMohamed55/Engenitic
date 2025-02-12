using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models.DTOs;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace GraduationProject.Controllers
{
    // TODO: Add Stages(number of videos and quizes), Requirements and Tags

    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly CoursesRepository _coursesRepo;

        public CoursesController(CoursesRepository coursesRepository)
        {
            _coursesRepo = coursesRepository;
        }



        [HttpGet("{index}")]
        public async Task<IActionResult> GetPageOfCourses(int index = 1)
        {
            if (index <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = System.Net.HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesRepo.GetPageOfCourses(index);

                if (index > courses.TotalPages)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid Page Number",
                        Code = System.Net.HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse()
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<CourseDTO>(courses),
                    Code = System.Net.HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = System.Net.HttpStatusCode.InternalServerError,
                });
            }

        }


        [HttpGet("search/{searchTerm}")]
        public async Task<IActionResult> GetCoursesBySearching(string searchTerm, int index = 1)
        {
            if (index <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = System.Net.HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesRepo.GetPageOfCoursesBySearching(searchTerm, index);

                if (courses.TotalPages == 0)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "No Courses Found.",
                        Code = System.Net.HttpStatusCode.NotFound,
                    });

                if (index > courses.TotalPages)
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid Page Number",
                        Code = System.Net.HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<CourseDTO>(courses),
                    Code = System.Net.HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = System.Net.HttpStatusCode.InternalServerError,
                });

            }
        }


        [HttpGet("id/{courseId}")]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            try
            {
                var course = await _coursesRepo.GetById(courseId);

                if (course == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Course Not Found.",
                        Code = System.Net.HttpStatusCode.NotFound,
                    });

                return Ok(new SuccessResponse()
                {
                    Message = "Course Retrieved Successfully.",
                    Data = course,
                    Code = System.Net.HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = System.Net.HttpStatusCode.InternalServerError,
                });
            }
        }
    }
}
