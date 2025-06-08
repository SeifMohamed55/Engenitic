using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
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
                    Data = new PaginatedResponse(courses),
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

            var enrollmentExists = await _studentService.EnrollmentExists(enrollment.StudentId, enrollment.CourseId);
            if (!enrollmentExists.Data)
            {
                try
                {
                    await _studentService.EnrollOnCourse(enrollment);
                }
                catch
                {
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "An error occured please try again later.",
                        Code = HttpStatusCode.BadRequest,
                    });
                }
            }
               
           var enrollmentResult = await _studentService.GetStudentEnrollment(enrollment.StudentId, enrollment.CourseId);
            if (!enrollmentResult.IsSuccess)
                return NotFound(new ErrorResponse()
                {
                    Message = enrollmentResult.Message ?? "",
                    Code = HttpStatusCode.NotFound,
                });

            return Ok(new SuccessResponse()
            {
                Message = enrollmentExists.Data ? "User already enrolled." : "User Enrolled Successfully",
                Data = enrollmentResult.Data,
                Code = HttpStatusCode.OK,
            });
        }

        // GET: /api/student/enrollment/current-stage
        [HttpGet("enrollment/current-stage")]
        public async Task<IActionResult> GetCurrentStage([FromQuery] int studentId, [FromQuery] int enrollmentId)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            if (!int.TryParse(claimId, out int parsedId) || parsedId != studentId)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });

            var res = await _studentService.GetEnrollmentCurrentStage(enrollmentId, studentId);
            return res.ToActionResult();
        }

        // GET: /api/student/enrollment
        [HttpGet("enrollment")]
        public async Task<IActionResult> GetCourseStage
            ([FromQuery] int studentId, [FromQuery] int enrollmentId, [FromQuery]int stage)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            if (!int.TryParse(claimId, out int parsedId) || parsedId != studentId)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });

            var res = await _studentService.GetEnrollmentStage(enrollmentId, stage, studentId);
            return res.ToActionResult();
        }

        // GET: /api/student/enrollment/attempt-quiz
        [HttpPost("enrollment/attempt-quiz")]
        public async Task<IActionResult> AttemptQuiz(UserQuizAttemptDTO userQuizAttempt)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            if (!int.TryParse(claimId, out int parsedId) || parsedId != userQuizAttempt.UserId)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var quizAttempt = await _studentService.AttemptQuiz(userQuizAttempt);
                if (!quizAttempt.TryGetData(out var data))
                    return BadRequest(new ErrorResponse()
                    {
                        Message = quizAttempt.Message,
                        Code = HttpStatusCode.BadRequest,
                    });

                var msg = data.IsPassed ? 
                    "Congratulations, you have passed the exam." :
                    "YOU HAVE FAILED!";

                return Ok(new SuccessResponse()
                {
                    Message = msg,
                    Data = data,
                    Code = HttpStatusCode.OK,
                });
            }
            catch(DbUpdateException)
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Quiz Structure",
                    Code = HttpStatusCode.BadRequest,
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Something Wrong Happend",
                    Code = HttpStatusCode.BadRequest,
                });
            }

        }


    }
}
