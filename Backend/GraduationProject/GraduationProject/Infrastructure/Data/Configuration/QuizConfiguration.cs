using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
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

            builder.Property(x => x.Position)
                .HasDefaultValue(null)
                .IsRequired();

            builder.Property(x => x.Description)
                .IsUnicode()
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasIndex(u => u.CourseId);
            builder.HasIndex(u => new { u.CourseId, u.Position });

            builder.HasMany(q => q.Questions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("now()");

        }
    }

}
