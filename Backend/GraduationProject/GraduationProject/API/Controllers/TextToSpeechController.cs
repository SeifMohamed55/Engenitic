using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.API.Controllers
{
    [Route("api/text-to-speech")]
    [ApiController]
    public class TextToSpeechController : ControllerBase
    {
        private readonly ITextToSpeechService _tts;
        public TextToSpeechController(ITextToSpeechService tts)
        {
            _tts = tts;
        }

        [HttpPost]
        public async Task<IActionResult> ConvertTextToSpeech([FromBody] TextToSpeechRequest text)
        {
            if (string.IsNullOrEmpty(text.Text))
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Text cannot be null or empty.",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }
            var audioStream = await _tts.GetAudioFromTextAsync(text.Text);
            if (audioStream.TryGetData(out var data))
            {
                return File(data, "audio/wav", "generated_speech.wav");

            }

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse()
            {
                Message =audioStream.Message,
                Code = System.Net.HttpStatusCode.BadRequest
            });
        }

    }
}
