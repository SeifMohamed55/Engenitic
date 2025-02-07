using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Data.Configuration
{
    public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
    {
        void IEntityTypeConfiguration<QuizQuestion>.Configure(EntityTypeBuilder<QuizQuestion> builder)
        {
            builder.ToTable("QuizQuestions");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.QuestionText)
                .HasMaxLength(600)
                .IsUnicode();

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.HasOne(x => x.Answer)
                .WithOne()
                .HasForeignKey<QuizQuestion>(x => x.AnswerId);

        }
    }

}
