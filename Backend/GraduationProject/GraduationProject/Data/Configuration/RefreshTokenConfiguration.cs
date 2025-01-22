using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Data.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        void IEntityTypeConfiguration<RefreshToken>.Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.LoginProvider)
                 .IsRequired()
                .HasMaxLength(120);

            builder.Property(u => u.Token)
                .IsRequired()
                .HasMaxLength(32);

        }
    }
}
