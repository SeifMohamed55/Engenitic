using GraduationProject.Controllers.ApiRequest;
using GraduationProject.Models.DTOs;

namespace GraduationProject.Services
{
    public class CourseGenerator
    {
        /*public static List<RegisterCourseRequest> GenerateCourses()
        {
            var courses = new List<RegisterCourseRequest>();

            for (int i = 0; i < 20; i++)
            {
                var course = new RegisterCourseRequest
                {
                    Code = $"C{i + 1:000}",
                    Title = $"Course Title {i + 1}",
                    Description = $"Comprehensive guide to {GetCourseTopic(i)}",
                    Requirements = $"Basic knowledge of {GetCourseTopic(i)} is recommended",
                    InstructorId = 4,
                    TagsStr = "",
                    //Image = null,
                    Tags = new List<TagDTO>(),
                    QuizesStr = "",
                    Quizes = GenerateQuizzes(i + 1, GetCourseTopic(i))
                };

                courses.Add(course);
            }

            return courses;
        }*/

        private static List<QuizDTO> GenerateQuizzes(int courseId, string topic)
        {
            var quizzes = new List<QuizDTO>();

            for (int i = 1; i <= 3; i++)
            {
                var quiz = new QuizDTO
                {
                    Title = $"{topic} Quiz {i}",
                    Position = i,
                    VideoUrl = GetYouTubeLink(courseId, i),
                    Questions = GenerateQuestions(topic, i)
                };

                quizzes.Add(quiz);
            }

            return quizzes;
        }

        private static List<QuestionDTO> GenerateQuestions(string topic, int quizId)
        {
            var questions = new List<QuestionDTO>();

            for (int i = 1; i <= 2; i++)
            {
                var question = new QuestionDTO
                {
                    QuestionText = GetQuestionText(topic, quizId, i),
                    Position = i,
                    Answers = GenerateAnswers(topic, quizId, i)
                };

                questions.Add(question);
            }

            return questions;
        }

        private static List<AnswerDTO> GenerateAnswers(string topic, int quizId, int questionId)
        {
            var answers = new List<AnswerDTO>();

            var correctAnswer = GetCorrectAnswer(topic, quizId, questionId);
            var incorrectAnswers = GetIncorrectAnswers(topic, quizId, questionId);

            answers.Add(new AnswerDTO { AnswerText = correctAnswer, IsCorrect = true, Position = 1 });

            for (int i = 2; i <= 4; i++)
            {
                answers.Add(new AnswerDTO { AnswerText = incorrectAnswers[i - 2], IsCorrect = false, Position = i });
            }

            return answers;
        }

        private static string GetCourseTopic(int index)
        {
            string[] topics = { "Python Programming", "Web Development", "Machine Learning", "Cyber Security", "Cloud Computing", "Mobile App Development", "Blockchain", "Game Development", "Database Management", "DevOps" };
            return topics[index % topics.Length];
        }

        private static string GetYouTubeLink(int courseId, int quizId)
        {
            string[] videoIds = { "3JZ_D3ELwOQ", "YlUKcNNmywk", "dQw4w9WgXcQ", "tgbNymZ7vqY", "hY7m5jjJ9mM" };
            return $"https://www.youtube.com/watch?v={videoIds[(courseId + quizId) % videoIds.Length]}";
        }

        private static string GetQuestionText(string topic, int quizId, int questionId)
        {
            string[,] questions =
            {
            { $"What is {topic} mainly used for?", "Which language is commonly used in {topic}?" },
            { "What is the primary goal of {topic}?", "Which tool is essential for {topic}?" }
        };
            return questions[quizId % 2, questionId % 2];
        }

        private static string GetCorrectAnswer(string topic, int quizId, int questionId)
        {
            return $"Correct answer for {topic} Quiz {quizId}, Question {questionId}";
        }

        private static List<string> GetIncorrectAnswers(string topic, int quizId, int questionId)
        {
            return new List<string>
        {
            $"Incorrect answer 1 for {topic} Quiz {quizId}, Question {questionId}",
            $"Incorrect answer 2 for {topic} Quiz {quizId}, Question {questionId}",
            $"Incorrect answer 3 for {topic} Quiz {quizId}, Question {questionId}"
        };
        }
    }

}
