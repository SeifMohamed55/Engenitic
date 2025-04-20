using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.HasIndex(x=> new { x.UserId, x.CourseId })
                .IsUnique();

            builder.HasIndex(x => x.Rating);

            builder.Property(r => r.Content)
                .IsRequired()
                .HasMaxLength(4096)
                .IsUnicode()
                .HasColumnType("citext");
        }
    }
}
