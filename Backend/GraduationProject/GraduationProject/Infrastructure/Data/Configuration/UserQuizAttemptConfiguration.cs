using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
{
    public class UserQuizAttemptConfiguration : IEntityTypeConfiguration<UserQuizAttempt>
    {
        void IEntityTypeConfiguration<UserQuizAttempt>.Configure(EntityTypeBuilder<UserQuizAttempt> builder)
        {
            builder.ToTable("UserQuizAttempts");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.CompletedAt)
                .HasDefaultValueSql("now()");

            builder.HasOne(x => x.Quiz)
                .WithMany()
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.UserAnswers)
                .WithOne()
                .HasForeignKey(x => x.UserQuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);



        }
    }

}
