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
            builder.HasKey(u =>  u.Id);

            builder.Property(u => u.AnswerText)
                .HasMaxLength(500)
                .IsUnicode();



        }
    }

}
