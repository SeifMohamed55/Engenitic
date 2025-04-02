using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "instructor")]
    public class InstructorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICloudinaryService _cloudinary;
        private readonly IUploadingService _uploadingService;
        public InstructorController
            (
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            ICloudinaryService cloudinary,
            IUploadingService uploadingService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _cloudinary = cloudinary;
            _uploadingService = uploadingService;
        }


        // GET: /api/instructor/1
        [HttpGet("courses")]
        public async Task<IActionResult> GetInstructorCourses([FromQuery] int index, [FromQuery] int instructorId)
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
                var courses = await _unitOfWork.CourseRepo.GetInstructorCourses(parsedId, index);
                if (courses.Count == 0)
                    return Ok(new SuccessResponse()
                    {
                        Message = "No Courses Found.",
                        Code = HttpStatusCode.OK,
                        Data = new PaginatedResponse<CourseDTO>(courses)
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
                var statistics = await _unitOfWork.CourseRepo.GetCourseStatistics(courseId);
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
                if (instructor == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid User.",
                        Code = HttpStatusCode.BadRequest,
                    });

                var defaultCourseHash = await _unitOfWork.FileHashRepo.GetDefaultCourseImageAsync();

                var addedCourse = await _unitOfWork.CourseRepo.MakeCourse(course, defaultCourseHash);
                await _unitOfWork.SaveChangesAsync();

                var imageName = "course_" + addedCourse.Id;

                using var stream = course.Image.OpenReadStream();

                var hash = await _uploadingService.UploadImageAsync(stream, imageName, CloudinaryType.CourseImage);

                if (hash == null)
                    hash = defaultCourseHash;

                addedCourse.FileHash = hash;

                await _unitOfWork.SaveChangesAsync();

                var dto = new CourseDTO(addedCourse);

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
        public async Task<IActionResult> EditCourse([FromForm] EditCourseRequest course)
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

                var addedCourse = await _unitOfWork.CourseRepo.EditCourse(course);
                await _unitOfWork.SaveChangesAsync();

                var resp = new CourseDTO(addedCourse);
                resp.Image.ImageURL = _cloudinary.GetImageUrl(resp.Image.ImageURL);

                return Ok(new SuccessResponse()
                {
                    Message = "Course Edited Successfully.",
                    Data = resp,
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

        [HttpPost("editCourseImage")]
        public async Task<IActionResult> EditCourseImage
            ([FromForm] IFormFile image, [FromForm] int courseId, [FromForm] int instructorId)
        {
            var addedCourse = await _unitOfWork.CourseRepo.GetById(courseId);
            if (addedCourse == null)
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
                parsedId != addedCourse.InstructorId
                )
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            try
            {
                var defaultCourseHash = await _unitOfWork.FileHashRepo.GetDefaultCourseImageAsync();

                var imageName = "course_" + addedCourse.Id;

                using var stream = image.OpenReadStream();

                var hash = await _uploadingService.UploadImageAsync(stream, imageName, CloudinaryType.CourseImage);

                if (hash == null)
                    hash = defaultCourseHash;

                if (!_unitOfWork.FileHashRepo.IsDefaultCourseImageHash(addedCourse.FileHash))
                    _unitOfWork.FileHashRepo.Delete(addedCourse.FileHash);

                addedCourse.FileHash = hash;

                await _unitOfWork.SaveChangesAsync();

                return Ok(new SuccessResponse()
                {
                    Message = "Image updated successfully.",
                    Code = HttpStatusCode.OK,
                    Data = new
                    {
                        Image = new ImageMetadata
                        {
                            ImageURL = _cloudinary.GetImageUrl(addedCourse.FileHash.PublicId),
                            Name = addedCourse.FileHash.PublicId.Split('/').LastOrDefault() ?? "",
                            Hash = addedCourse.FileHash.Hash
                        }
                    }
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

        

        [HttpDelete("deleteCourse")]
        public async Task<IActionResult> DeleteCourse(DeleteCourseRequest req)
        {
            var course = await _unitOfWork.CourseRepo.GetById(req.CourseId);
            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (claimId == null || !int.TryParse(claimId.Value, out int instructorId) || course == null)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Invalid User request."
                });

            if (course.InstructorId != instructorId || course.InstructorId != req.InstructorId)
                return StatusCode(403, new ErrorResponse()
                {
                    Code = HttpStatusCode.Forbidden,
                    Message = "insufficient permessions."
                });
            try
            {
                course.hidden = true;
                await _unitOfWork.SaveChangesAsync();
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
