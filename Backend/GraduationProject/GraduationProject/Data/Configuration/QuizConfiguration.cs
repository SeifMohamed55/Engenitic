using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Data.Configuration
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        void IEntityTypeConfiguration<Quiz>.Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.ToTable("Quizes");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Title)
                .HasMaxLength(100)
                .IsUnicode();

            builder.HasIndex(u => u.Position);

            builder.HasIndex(u => u.CourseId);

            builder.HasMany(q => q.Questions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("now()");

        }
    }

}
