using GraduationProject.Controllers.ApiRequest;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "student")]
    public class StudentController : ControllerBase
    {

        private readonly IEnrollmentRepository _enrollmentRepository;

        public StudentController(IEnrollmentRepository enrollmentRepository) 
        {
            _enrollmentRepository = enrollmentRepository;
        }

        // GET: /api/student/courses
        [HttpGet("courses")]
        public async Task<IActionResult> GetStudentCourses([FromQuery]int index, [FromQuery]int id)
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
                if(!int.TryParse(claimId, out int parsedId) || parsedId != id)
                    return Unauthorized(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.Unauthorized,
                    });

                var courses = await _enrollmentRepository.GetStudentEnrolledCourses(parsedId, index);
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
                return Unauthorized(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.Unauthorized,
                });
            var result = await _enrollmentRepository.EnrollOnCourse(enrollment);
            if (result == false)
                return NotFound(new ErrorResponse()
                {
                    Message = "An Error occured, Please Try again later.",
                    Code = HttpStatusCode.NotFound,
                });
            return Ok(new SuccessResponse()
            {
                Message = "Enrolled Successfully.",
                Data = result,
                Code = HttpStatusCode.OK,
            });
        }

    }
}
