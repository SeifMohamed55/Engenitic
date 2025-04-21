using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="student")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IStudentService _studentService;
        private readonly IJwtTokenService _jwtTokenService;
        public ReviewsController
            (IReviewService reviewService, IStudentService studentService, IJwtTokenService jwtTokenService)
        {
            _reviewService = reviewService;
            _studentService = studentService;
            _jwtTokenService = jwtTokenService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetReviewsByCourseId(int courseId, int index = 1)
        {
            // extract userId from Authorization
            int? userId = null;
            string? oldAccessToken = _jwtTokenService.ExtractJwtTokenFromContext(HttpContext);

            if (oldAccessToken != null)
                (userId, _) = _jwtTokenService.ExtractIdAndJtiFromExpiredToken(oldAccessToken);

            var result = await _reviewService.GetReviewsByCourseIdAsync(courseId, userId, index);
            if (result.TryGetData(out var data))
                return Ok(new SuccessResponse()
                {
                    Message = "Reviews retrieved successfully",
                    Data = new PaginatedResponse<ReviewDTO>(data),
                    Code = HttpStatusCode.OK,
                });
            return BadRequest(new ErrorResponse()
            {
                Code = HttpStatusCode.BadRequest,
                Message = result.Message
            });
        }

        // Add review
        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequestModel review)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });

            if (!int.TryParse(claimId, out int parsedId))
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });

            var isEnrolledResult = await _studentService.EnrollmentExists(parsedId, review.CourseId);

            if (!isEnrolledResult.IsSuccess)
                return BadRequest(new ErrorResponse()
                {
                    Message = isEnrolledResult.Message,
                    Code = HttpStatusCode.BadRequest,
                });

            if(!isEnrolledResult.Data)
                return BadRequest(new ErrorResponse()
                {
                    Message = "You are not enrolled in this course.",
                    Code = HttpStatusCode.BadRequest,
                });


            var result = await _reviewService.AddReviewAsync(parsedId, review);
            if (result.TryGetData(out var data))
                return Ok(new SuccessResponse()
                {
                    Message = "Review added successfully",
                    Data = data,
                    Code = HttpStatusCode.OK,
                });
            return BadRequest(new ErrorResponse()
            {
                Code = HttpStatusCode.BadRequest,
                Message = result.Message
            });
        }

        // Edit review
        [HttpPost("edit")]
        public async Task<IActionResult> EditReview([FromBody] EditReviewRequestModel review)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            if (!int.TryParse(claimId, out int parsedId))
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            var result = await _reviewService.EditReviewAsync(parsedId, review);
            if (result.TryGetData(out var data))
                return Ok(new SuccessResponse()
                {
                    Message = "Review edited successfully",
                    Data = data,
                    Code = HttpStatusCode.OK,
                });
            return BadRequest(new ErrorResponse()
            {
                Code = HttpStatusCode.BadRequest,
                Message = result.Message
            });
        }

        // Delete review
        [HttpPost("delete/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            if (!int.TryParse(claimId, out int parsedId))
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User.",
                    Code = HttpStatusCode.BadRequest,
                });
            var result = await _reviewService.DeleteReviewAsync(parsedId, reviewId);
            if (result.TryGetData(out var data))
                return Ok(new SuccessResponse()
                {
                    Message = "Review deleted successfully",
                    Data = data,
                    Code = HttpStatusCode.OK,
                });
            return BadRequest(new ErrorResponse()
            {
                Code = HttpStatusCode.BadRequest,
                Message = result.Message
            });
        }


    }
}
