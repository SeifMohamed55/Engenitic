namespace GraduationProject.Models.DTOs
{
    public class QuizDTO
    {

       public int Id { get; set; }
       public string Title { get; set; } = null!;
       public int Position { get; set; }
       public IEnumerable<QuestionDTO> Questions { get; set; } = null!;

    }
}
