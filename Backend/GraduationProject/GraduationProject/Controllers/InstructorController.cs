using GraduationProject.Controllers.ApiRequest;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using GraduationProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "instructor")]
    public class InstructorController : ControllerBase
    {
        private readonly ICourseRepository _coursesRepo;
        private readonly UserManager<AppUser> _userManager;
        public InstructorController(ICourseRepository courseRepository, UserManager<AppUser> userManager) 
        {
            _coursesRepo = courseRepository;
            _userManager = userManager;
        }


        // GET: /api/instructor/1
        [HttpGet("courses")]
        public async Task<IActionResult> GetInstructorCourses([FromQuery]int index, [FromQuery]int instructorId)
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
                if (!int.TryParse(claimId, out int parsedId) || parsedId != instructorId)
                    return Unauthorized(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.Unauthorized,
                    });
                var courses = await _coursesRepo.GetInstructorCourses(parsedId, index);
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

        // GET: /api/instructor/statistics/1
        [HttpGet("statistics/{courseId}")]
        [Authorize(Roles = "instructor, admin")]
        public async Task<IActionResult> GetCourseStatistics(int courseId)
        {
            try
            {
                var statistics = await _coursesRepo.GetCourseStatistics(courseId);
                if (statistics == null)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "Course was not found.",
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

        // POST: Add Course

        [HttpPost("addCourse")]
        public async Task<IActionResult> AddCourse([FromForm] RegisterCourseRequest course)
        
        {

            List<QuizDTO>? quizes;
            List<TagDTO>? tags;
            try
            {
                quizes = JsonConvert.DeserializeObject<List<QuizDTO>>(course.QuizesStr);
                if (quizes == null)
                    throw new JsonException("Nothing Sent");
            }
            catch (JsonException)
            {
                return BadRequest(new ErrorResponse() 
                {
                    Message = "Invalid Quizes.",
                    Code = HttpStatusCode.BadRequest,
                });
            }

            try
            {
                tags = JsonConvert.DeserializeObject<List<TagDTO>>(course.TagsStr);
                if(tags == null)
                    throw new JsonException("Nothing Sent");

            }
            catch{ tags = new List<TagDTO>(); }

            course.Quizes = quizes;
            course.QuizesStr = "Useless!";

            course.Tags = tags;
            course.TagsStr = "useless";

            var validationResults = new List<ValidationResult>();

            var context = new ValidationContext(course);
            if (!Validator.TryValidateObject(course, context, validationResults, true))
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = validationResults,
                    Code = HttpStatusCode.BadRequest,
                });
            }                

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
                    return Unauthorized(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.Unauthorized,
                    });

                var instructor = await _userManager.FindByIdAsync(claimId);
                if(instructor == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.BadRequest,
                    });

                var addedCourse = await _coursesRepo.AddCourse(course);

                return Ok(new SuccessResponse()
                {
                    Message = "Course Added Successfully.",
                    Data = addedCourse,
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
