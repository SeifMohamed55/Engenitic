using Microsoft.AspNetCore.Identity;
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

            builder.Property(u => u.PhoneRegionCode).HasMaxLength(5);

            builder.HasIndex(x => x.NormalizedEmail).IsUnique();

            builder.Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.UserName)
                .IsRequired()
                .IsUnicode()
                .HasMaxLength(120);

            builder.Property(u => u.FullName)
                .IsRequired()
                .IsUnicode()
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber).HasMaxLength(25);

            builder.HasMany(x => x.Roles)
                .WithMany(x => x.Users)
                .UsingEntity<IdentityUserRole<int>>();

            builder.HasOne(x => x.RefreshToken)
                .WithOne(x => x.AppUser)
                .HasForeignKey<AppUser>(x => x.RefreshTokenId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }


}
