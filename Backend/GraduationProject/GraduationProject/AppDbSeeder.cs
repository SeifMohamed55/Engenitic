using GraduationProject.Models;

namespace GraduationProject
{
    public static class AppDbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Courses.Any())
            {
                var course = new Course { Title = "C# Basics", Description = "Learn C# from scratch" };
                context.Courses.Add(course);
                context.SaveChanges();

                var quiz = new Quiz { CourseId = course.Id, Title = "C# Basics Quiz" };
                context.Quizzes.Add(quiz);
                context.SaveChanges();

                var question = new QuizQuestion { QuizId = quiz.Id, QuestionText = "What is the main entry point in a C# program?" };
                context.QuizQuestions.Add(question);
                context.SaveChanges();

                context.QuizAnswers.AddRange(
                    new QuizAnswer { QuestionId = question.Id, AnswerText = "Main()", IsCorrect = true },
                    new QuizAnswer { QuestionId = question.Id, AnswerText = "Start()", IsCorrect = false },
                    new QuizAnswer { QuestionId = question.Id, AnswerText = "Run()", IsCorrect = false }
                );

                context.SaveChanges();
            }
        }
    }


}
