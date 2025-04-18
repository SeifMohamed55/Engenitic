using System.Collections.Frozen;

namespace GraduationProject.Domain.DTOs
{
    public class QuizQuestionAnswerIds
    {

        public IReadOnlySet<int> QuizzesIds { get; set; } = new HashSet<int>();
        public IReadOnlySet<int> QuestionIds { get; set; } = new HashSet<int>();
        public IReadOnlySet<int> AnswerIds { get; set; } = new HashSet<int>();
    }
}
