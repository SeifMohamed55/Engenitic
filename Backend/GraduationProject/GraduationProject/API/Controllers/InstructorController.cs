using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "instructor")]
    public class InstructorController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICoursesService _coursesService;
        public InstructorController
            (
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            ICloudinaryService cloudinary,
            IUploadingService uploadingService,
            ICoursesService coursesService)
        {
            _userManager = userManager;
            _coursesService = coursesService;
        }


        // GET: /api/instructor/1
        [HttpGet("courses")]
        public async Task<IActionResult> GetInstructorCourses([FromQuery] int instructorId, [FromQuery] int index = 1)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });

            if (!int.TryParse(claimId, out int parsedId) || parsedId != instructorId)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            var res = await _coursesService.GetInstructorCourses(parsedId, index);

            return res.ToActionResult();
           
        }

        // GET: /api/instructor/statistics/1
        [HttpGet("statistics/{courseId}")]
        [Authorize(Roles = "instructor, admin")]
        public async Task<IActionResult> GetCourseStatistics(int courseId)
        {

            var res = await _coursesService.GetCourseStatistics(courseId);
            return res.ToActionResult();
            
        }

        // POST: Add Course

        [HttpPost("addCourse")]
        public async Task<IActionResult> AddCourse([FromForm] RegisterCourseRequest course)
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
                if (!int.TryParse(claimId, out int parsedId) || parsedId != course.InstructorId)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.BadRequest,
                    });

                var instructor = await _userManager.FindByIdAsync(claimId);
                if (instructor == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.BadRequest,
                    });

                var dto = await _coursesService.AddCourse(course);
                return Ok(new SuccessResponse()
                {
                    Message = "Course Added Successfully.",
                    Data = dto,
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


        [HttpPost("editCourse")]
        public async Task<IActionResult> EditCourse([FromBody] EditCourseRequest course)
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
                if (!int.TryParse(claimId, out int parsedId) || parsedId != course.InstructorId)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.BadRequest,
                    });

                var instructor = await _userManager.FindByIdAsync(claimId);
                if (instructor == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.BadRequest,
                    });

                var dto = await _coursesService.EditCourse(course);

                return dto.ToActionResult();

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

        [HttpPost("editCourseImage")]
        public async Task<IActionResult> EditCourseImage
            ([FromForm] IFormFile image, [FromForm] int courseId, [FromForm] int instructorId)
        {
            var courseInstructorId = await _coursesService.GetCourseInstructorId(courseId);
            if (!courseInstructorId.HasValue)
                return NotFound(new ErrorResponse()
                {
                    Code = HttpStatusCode.NotFound,
                    Message = "Course Not found."
                });

            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (
                claimId == null ||
                !int.TryParse(claimId, out int parsedId) ||
                parsedId != instructorId ||
                parsedId != courseInstructorId
                )
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var res = await _coursesService.EditCourseImage(image, courseId);
                return res.ToActionResult();
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

        [HttpGet("course-with-quizes")]
        public async Task<IActionResult> GetCourseWithQuizes([FromQuery] int courseId)
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
                if (!int.TryParse(claimId, out int parsedId))
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.BadRequest,
                    });
                var course = await _coursesService.GetCourseWithQuizes(courseId);
                if (course == null)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "Course was not found.",
                        Code = HttpStatusCode.NotFound,
                    });
                return course.ToActionResult();
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



        [HttpDelete("deleteCourse")]
        public async Task<IActionResult> DeleteCourse(DeleteCourseRequest req)
        {
            var dbInstructorId = await _coursesService.GetCourseInstructorId(req.CourseId);
            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (claimId == null || !int.TryParse(claimId.Value, out int instructorId) || !dbInstructorId.HasValue)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Invalid User request."
                });

            if (dbInstructorId != instructorId || dbInstructorId != req.InstructorId)
                return StatusCode(403, new ErrorResponse()
                {
                    Code = HttpStatusCode.Forbidden,
                    Message = "insufficient permessions."
                });
            try
            {
                await _coursesService.DeleteCourse(req.CourseId);
                return Ok(new SuccessResponse()
                {
                    Code = HttpStatusCode.OK,
                    Message = "Course Deleted Successfully",
                    Data = true
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Something wrong happend, please try again later"
                });
            }


        }

        /*
                [AllowAnonymous]
                [HttpPost("addDummyCourses")]
                public async Task<IActionResult> AddDummyCourses()
                {

                    var courses = CourseGenerator.GenerateCourses();

                    var result = await _coursesRepo.AddListOfCourses(courses);




                    return Ok(result);
                }*/

    }
}
