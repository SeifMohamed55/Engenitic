using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Data.Configuration
{
    public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
    {
        void IEntityTypeConfiguration<QuizAnswer>.Configure(EntityTypeBuilder<QuizAnswer> builder)
        {
            builder.ToTable("QuizAnswers");
            builder.HasKey(u =>  u.QuestionId);

            builder.Property(u => u.AnswerText)
                .HasMaxLength(500)
                .IsUnicode();

            builder.HasOne(x=> x.Question)
                .WithOne(x => x.Answer)
                .HasForeignKey<QuizAnswer>(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }

}
