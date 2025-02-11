using GraduationProject.Models;

namespace GraduationProject.Services
{
    public static class AppDbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Courses.Any())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var course = new Course()
                        {
                            InstructorId = 4,
                            Title = "C# Basics",
                            Description = "Learn C# from scratch",
                            Code = "C#101",
                            ImageUrl = "https://via.placeholder.com/150",
                            hidden = false,
                        };
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
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                    }
                }
            }
        }
    }
}
