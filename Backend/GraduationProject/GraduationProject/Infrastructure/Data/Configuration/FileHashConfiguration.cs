using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
{
    public class FileHashConfiguration : IEntityTypeConfiguration<FileHash>
    {
        void IEntityTypeConfiguration<FileHash>.Configure(EntityTypeBuilder<FileHash> builder)
        {
            builder.ToTable("FileHash");

            builder.HasKey(u => u.Id);

            builder.Property(i => i.Type)
            .HasConversion<int>();

            builder.HasIndex(x => x.PublicId)
                .IsUnique();


        }
    }

}
