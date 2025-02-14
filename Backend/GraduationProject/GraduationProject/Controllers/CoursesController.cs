using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models.DTOs;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net;

namespace GraduationProject.Controllers
{
    // TODO: Add Stages(number of videos and quizes), Requirements and Tags

    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseRepository _coursesRepo;

        public CoursesController(ICourseRepository coursesRepository)
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


        [HttpGet("search")]
        public async Task<IActionResult> GetCoursesBySearching
                                        ([FromQuery(Name = "search")]string search, [FromQuery]int page = 1)
        {
            if (page <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = System.Net.HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesRepo.GetPageOfCoursesBySearching(search, page);

                if (courses.TotalPages == 0)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "No Courses Found.",
                        Code = System.Net.HttpStatusCode.NotFound,
                    });

                if (page > courses.TotalPages)
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


        // GET: /api/courses/student/1
        [HttpGet("student/{index}")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> GetStudentCourses(int index)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesRepo.GetStudentEnrolledCourses(int.Parse(claimId), index);
                if (courses.Count == 0)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "No Courses Found.",
                        Code = HttpStatusCode.NotFound,
                    });
                return Ok(new SuccessResponse()
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<EnrollmentDTO>(courses),
                    Code = HttpStatusCode.OK,
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.BadRequest,
                });
            }
        }

        // GET: search for instructor
        // GET: /api/users/instructor/1
        [HttpGet("instructor/{index}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> GetInstructorCourses(int index)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesRepo.GetInstructorCourses(int.Parse(claimId), index);
                if (courses.Count == 0)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "No Courses Found.",
                        Code = HttpStatusCode.NotFound,
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
                return BadRequest(new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.BadRequest,
                });
            }
        }

        // GET: /api/courses/statistics/1
        [HttpGet("statistics/{courseId}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> GetCourseStatistics(int courseId)
        {
            try
            {
                var statistics = await _coursesRepo.GetCourseStatistics(courseId);
                if (statistics == null)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "No Statistics Found.",
                        Code = HttpStatusCode.NotFound,
                    });
                return Ok(new SuccessResponse()
                {
                    Message = "Statistics Retrieved Successfully.",
                    Data = statistics,
                    Code = HttpStatusCode.OK,
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = HttpStatusCode.BadRequest,
                });
            }
        }
    }
}
