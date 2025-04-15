namespace GraduationProject.API.Responses
{
    public class GrammarCorrectionResponse
    {
        public string CorrectedText { get; set; } = string.Empty;
        public float Score { get; set; }
    }
}
