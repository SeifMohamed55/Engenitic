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

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public StudentController(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
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

                var courses = await _unitOfWork.EnrollmentRepo.GetStudentEnrolledCourses(parsedId, index);
                courses.ForEach(x =>
                {
                    x.Course.Image.ImageURL = _cloudinaryService
                     .GetImageUrl(x.Course.Image.ImageURL, x.Course.Image.Version);
                });

                if (index > courses.TotalPages && courses.TotalPages != 0)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid Page Number",
                        Code = HttpStatusCode.BadRequest,
                    });

                /*if (courses.Count == 0)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "No Courses Found.",
                        Code = HttpStatusCode.NotFound,
                    });
                */
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
            int enrollmentId = 0;
            try
            {
                await _unitOfWork.EnrollmentRepo.EnrollOnCourse(enrollment);
                await _unitOfWork.SaveChangesAsync();

                var enrollmentDTO = await _unitOfWork.EnrollmentRepo
                    .GetStudentEnrollment(enrollment.StudentId, enrollment.CourseId);
                if (enrollmentDTO == null)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "Enrollment Not Found.",
                        Code = HttpStatusCode.NotFound,
                    });

                enrollmentDTO.Course.Image.ImageURL = _cloudinaryService
                    .GetImageUrl(enrollmentDTO.Course.Image.ImageURL, enrollmentDTO.Course.Image.Version);

                enrollmentId = enrollmentDTO.Id;

                return Ok(new SuccessResponse()
                {
                    Message = "Enrolled Successfully.",
                    Data = enrollmentDTO,
                    Code = HttpStatusCode.OK,
                });
            }
            catch (DbUpdateException)
            {
                var enrollmentDTO = await _unitOfWork.EnrollmentRepo
                    .GetStudentEnrollment(enrollment.StudentId, enrollment.CourseId) ?? new EnrollmentDTO();

                enrollmentDTO.Course.Image.ImageURL = _cloudinaryService
                    .GetImageUrl(enrollmentDTO.Course.Image.ImageURL, enrollmentDTO.Course.Image.Version);

                return Ok(new SuccessResponse()
                {
                    Code = HttpStatusCode.OK,
                    Message = "User Already Enrolled.",
                    Data = enrollmentDTO
                });
            }
            catch (Exception ex)
            {
                return NotFound(new ErrorResponse()
                {
                    Message = ex.Message,
                    Code = HttpStatusCode.NotFound,
                });
            }

        }

    }
}
