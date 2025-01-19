using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace GraduationProject.Data.Configuration
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        void IEntityTypeConfiguration<AppUser>.Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Address).HasMaxLength(100);
            builder.Property(u => u.Country).HasMaxLength(20);

            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.UserName)
                .IsRequired()
                .IsUnicode()
                .HasMaxLength(80);
            builder.Property(u => u.PhoneNumber).HasMaxLength(20);

        }
    }
}
