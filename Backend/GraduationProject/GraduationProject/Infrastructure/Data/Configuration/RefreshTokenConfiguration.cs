using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        void IEntityTypeConfiguration<RefreshToken>.Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => new { x.DeviceId, x.UserId });

            builder.Property(x => x.DeviceId)
                .ValueGeneratedNever();

            builder.Property(x=> x.IssuedAt)
                .HasDefaultValueSql("NOW()");


            builder.Property(u => u.LoginProvider)
                 .IsRequired()
                .HasMaxLength(120);

        }
    }
}
