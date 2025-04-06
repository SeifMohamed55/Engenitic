using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "student")]
    public class StudentController : ControllerBase
    {

        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: /api/student/courses
        [HttpGet("courses")]
        public async Task<IActionResult> GetStudentCourses([FromQuery] int id, [FromQuery] int index = 1)
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
                if (!int.TryParse(claimId, out int parsedId) || parsedId != id)
                    return Unauthorized(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.Unauthorized,
                    });

                var courses = await _studentService.GetStudentEnrollments(id, index);

                if (index > courses.TotalPages && courses.TotalPages != 0)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid Page Number",
                        Code = HttpStatusCode.BadRequest,
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


        // POST: /api/student/enroll
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll(StudentEnrollmentRequest enrollment)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });

            if (!int.TryParse(claimId, out int parsedId) || parsedId != enrollment.StudentId)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            string msg = "Enrolled Successfully.";
            try
            {
                await _studentService.EnrollOnCourse(enrollment);
            }
            catch (DbUpdateException)
            {
                msg = "User already enrolled.";
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "An error occured please try again later.",
                    Code = HttpStatusCode.BadRequest,
                });
            }

           var enrollmentDTO = await _studentService.GetStudentEnrollment(enrollment.StudentId, enrollment.CourseId);
            if (!enrollmentDTO.IsSuccess)
                return NotFound(new ErrorResponse()
                {
                    Message = enrollmentDTO.Error ?? "",
                    Code = HttpStatusCode.NotFound,
                });

            return Ok(new SuccessResponse()
            {
                Message = msg,
                Data = enrollmentDTO.Data,
                Code = HttpStatusCode.OK,
            });
        }

    }
}
