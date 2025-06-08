using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Extensions;
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
            var res = await _grammarCorrectionService.CorrectGrammar(request);
            return res.ToActionResult();
        }
    }
}
