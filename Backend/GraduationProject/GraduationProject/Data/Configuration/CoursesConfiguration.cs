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

            builder.HasIndex(u => u.Title);
            builder.Property(u => u.Title)
                .HasMaxLength(100)
                .IsUnicode()
                .HasColumnType("citext");

            builder.HasIndex(x => x.Description);
            builder.Property(u => u.Description)
                .IsUnicode()
                .HasColumnType("citext");

            builder.Property(u => u.Code)
                .HasMaxLength(10);

            builder.HasOne(x => x.Instructor)
                .WithMany(x=> x.Courses)
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }


}
