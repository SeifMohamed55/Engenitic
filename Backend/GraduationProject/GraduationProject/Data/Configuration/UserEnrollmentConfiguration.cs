using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace GraduationProject.Data.Configuration
{
    public class UserEnrollmentConfiguration : IEntityTypeConfiguration<UserEnrollment>
    {
        void IEntityTypeConfiguration<UserEnrollment>.Configure(EntityTypeBuilder<UserEnrollment> builder)
        {
            builder.ToTable("UserEnrollments");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.EnrolledAt)
                .HasDefaultValueSql("now()");

            builder.HasMany(x=> x.QuizAttempts)
                .WithOne(x => x.UserEnrollment)
                .HasForeignKey(x => x.UserEnrollmentId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }

}
