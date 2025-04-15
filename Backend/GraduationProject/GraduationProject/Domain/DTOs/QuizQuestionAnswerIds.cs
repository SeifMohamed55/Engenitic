using System.Collections.Frozen;

namespace GraduationProject.Domain.DTOs
{
    public class QuizQuestionAnswerIds
    {


        public FrozenSet<int> QuestionIds { get; set; } =  FrozenSet<int>.Empty;
        public FrozenSet<int> AnswerIds { get; set; } = FrozenSet<int>.Empty;
    }
}
