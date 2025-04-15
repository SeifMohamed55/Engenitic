using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class GrammarCorrectionRequest
    {
        [Required(ErrorMessage = "Sentence is required.")]
        public string Sentence { get; set; } = null!;
    }

}
