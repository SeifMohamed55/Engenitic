﻿using GraduationProject.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Infrastructure.Data.Configuration
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

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(u => u.PhoneNumber).HasMaxLength(25);

            builder.HasMany(x => x.Roles)
                .WithMany(x => x.Users)
                .UsingEntity<IdentityUserRole<int>>();

            builder.HasMany(x => x.RefreshTokens)
                .WithOne(x => x.AppUser)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Courses)
                .WithOne(x => x.Instructor)
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Enrollments)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.FileHashes)
                .WithMany(x => x.Users)
                .UsingEntity("AppUserFileHash");

            builder.HasMany(x => x.Reviews)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }


}
