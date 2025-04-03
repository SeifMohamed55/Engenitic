using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
{
    public class UserEnrollmentConfiguration : IEntityTypeConfiguration<UserEnrollment>
    {
        void IEntityTypeConfiguration<UserEnrollment>.Configure(EntityTypeBuilder<UserEnrollment> builder)
        {
            builder.ToTable("UserEnrollments");
            builder.HasKey(u => u.Id);

            builder.HasIndex(x=> new {x.CourseId, x.UserId })
                .IsUnique();

            builder.Property(u => u.EnrolledAt)
                .HasDefaultValueSql("now()");

            builder.HasMany(x => x.QuizAttempts)
                .WithOne(x => x.UserEnrollment)
                .HasForeignKey(x => x.UserEnrollmentId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }

}
