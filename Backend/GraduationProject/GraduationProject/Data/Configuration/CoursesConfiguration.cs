using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Data.Configuration
{
    public class CoursesConfiguration : IEntityTypeConfiguration<Course>
    {
        void IEntityTypeConfiguration<Course>.Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Title)
                .HasMaxLength(100)
                .IsUnicode();

            builder.Property(u => u.Description)
                .IsUnicode();

            builder.Property(u => u.Code)
                .HasMaxLength(10);

            builder.HasOne(x => x.Instructor)
                .WithOne()
                .HasForeignKey<Course>(Course => Course.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }


}
