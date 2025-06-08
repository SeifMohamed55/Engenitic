﻿using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GraduationProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VqaController : ControllerBase
    {
        private readonly IVqaService _vqaService;

        // ✅ Inject VqaService via constructor
        public VqaController(IVqaService vqaService)
        {
            _vqaService = vqaService;
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromForm] IFormFile image, [FromForm] string question)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image file is required.");

            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Question is required.");

            if (!UploadingService.IsValidImageType(image, 10 * 1024 * 1024))
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Image is invalid.",
                    Code = HttpStatusCode.UnsupportedMediaType
                });
            }

            var answer = await _vqaService.GetAnswerAsync(image, question);
            return answer.ToActionResult();
        }
    }

}
