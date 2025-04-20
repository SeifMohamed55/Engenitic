using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.API.Controllers
{
    // TODO: Tags

    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesService _coursesService;
        private readonly IStudentService  _studentService;
        private readonly IJwtTokenService _jwtTokenService;

        public CoursesController
            (ICoursesService coursesService, IStudentService studentService, IJwtTokenService jwtTokenService)
        {
            _coursesService = coursesService;
            _studentService = studentService;
            _jwtTokenService = jwtTokenService;

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

                if (index > courses.TotalPages && courses.TotalPages != 0)
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

                if (index > courses.TotalPages && courses.TotalPages != 0)
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

                if (index > courses.TotalPages && courses.TotalPages != 0)
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
            // extract userId from Authorization
            int userId = 0;
            string? oldAccessToken = _jwtTokenService.ExtractJwtTokenFromContext(HttpContext);

            if(oldAccessToken != null)
                (userId, _) = _jwtTokenService.ExtractIdAndJtiFromExpiredToken(oldAccessToken);

            try
            {
                var isEnrolled = await _studentService.EnrollmentExists(userId, courseId);

                var course = await _coursesService.GetCourseDetailsById(courseId);

                if (course == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Course Not Found.",
                        Code = HttpStatusCode.NotFound,
                    });

                course.IsEnrolled = isEnrolled.Data;

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

        // GET: /api/courses/quizzes-title
        [HttpGet("quizzes-title")]
        public async Task<IActionResult> GetCourseQuizesTiltles
            ([FromQuery] int courseId)
        {
            var quizesTitle = await _coursesService.GetQuizesTitles(courseId);
            if (!quizesTitle.IsSuccess)
                return BadRequest(new ErrorResponse()
                {
                    Message = quizesTitle.Message,
                    Code = HttpStatusCode.BadRequest,   
                });
            return Ok(new SuccessResponse()
            {
                Message = "Quizes Titles Retrieved Successfully.",
                Data = quizesTitle.Data,
                Code = HttpStatusCode.OK,
            });
        }


        // GET: /api/courses/random4
        [HttpGet("random4")]
        public async Task<IActionResult> GetRandomCourses()
        {
            try
            {
                var courses = await _coursesService.GetRandomCourses(4);
                return Ok(new SuccessResponse()
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = courses,
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
