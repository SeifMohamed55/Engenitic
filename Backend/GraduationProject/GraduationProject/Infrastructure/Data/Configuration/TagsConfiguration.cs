using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
{
    public class TagsConfiguration : IEntityTypeConfiguration<Tag>
    {
        //  many to many relationship is in courses
        void IEntityTypeConfiguration<Tag>.Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Value);

            builder.Property(u => u.Value)
                 .IsRequired()
                 .IsUnicode()
                 .HasColumnType("citext")
                 .HasMaxLength(150);



        }
    }
}
