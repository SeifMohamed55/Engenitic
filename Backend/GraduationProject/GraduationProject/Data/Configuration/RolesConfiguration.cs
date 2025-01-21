using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Data.Configuration
{
    public class RolesConfiguration : IEntityTypeConfiguration<Role>
    {
        void IEntityTypeConfiguration<Role>.Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasDefaultValue(null);

        }
    }
}
