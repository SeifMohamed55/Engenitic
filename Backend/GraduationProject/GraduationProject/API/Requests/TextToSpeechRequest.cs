using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class TextToSpeechRequest
    {
        [Required]
        public string Text { get; set; } = null!;
    }
}
