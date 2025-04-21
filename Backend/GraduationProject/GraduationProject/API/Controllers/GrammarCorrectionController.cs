using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrammarCorrectionController : ControllerBase
    {
        private readonly IGrammarCorrectionService _grammarCorrectionService;
        public GrammarCorrectionController(IGrammarCorrectionService grammarCorrectionService)
        {
            _grammarCorrectionService = grammarCorrectionService;
        }
        [HttpPost("correct")]
        public async Task<IActionResult> CorrectGrammar(GrammarCorrectionRequest request)
        {
            try
            {
                var res = await _grammarCorrectionService.CorrectGrammar(request);
                if (res.TryGetData(out var data))
                {
                    return Ok(new SuccessResponse()
                    {
                        Message = "Grammar Corrected Successfully.",
                        Data = data,
                        Code = System.Net.HttpStatusCode.OK
                    });
                }
                else
                {
                    return BadRequest(new ErrorResponse()
                    {
                        Message = res.Message ?? "An error occurred while correcting grammar.",
                        Code = System.Net.HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse()
                {
                    Message = ex.Message,
                    Code = System.Net.HttpStatusCode.InternalServerError
                });
            }
        }
    }
}
