using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GraduationProject.API.Controllers
{

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

            var res = await _coursesService.GetPageOfCourses(index);
            return res.ToActionResult();
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

            var res = await _coursesService.SearchOnPageOfCourses(search, index);
            return res.ToActionResult();       
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

            var res = await _coursesService.GetPageOfCoursesByTag(tag, index);
            return res.ToActionResult();
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

            var isEnrolled = await _studentService.EnrollmentExists(userId, courseId);

            var courseRes = await _coursesService.GetCourseDetailsById(courseId);

            if (!courseRes.TryGetData(out var course))
                return NotFound(new ErrorResponse
                {
                    Message = courseRes.Message,
                    Code = HttpStatusCode.NotFound,
                });

            course.IsEnrolled = isEnrolled.Data;

            return courseRes.ToActionResult();

        }

        // GET: /api/courses/quizzes-title
        [HttpGet("quizzes-title")]
        public async Task<IActionResult> GetCourseQuizesTiltles
            ([FromQuery] int courseId)
        {
            var res = await _coursesService.GetQuizesTitles(courseId);
            return res.ToActionResult();
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
