using System.Collections.Generic;
using System.Net;
using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Presentation.Models;

namespace Presentaion.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Custom entities
    public DbSet<ProfileEntity> Profiles => Set<ProfileEntity>();
    public DbSet<AddressEntity> Addresses => Set<AddressEntity>();
    public DbSet<EmailVfTokenEntity> EmailVerificationTokens => Set<EmailVfTokenEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        const string prefix = "auth_"; // CHANGE if needed

        // Identity tables
        builder.Entity<ApplicationUser>().ToTable(prefix + "Users");
        builder.Entity<ApplicationRole>().ToTable(prefix + "Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable(prefix + "UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable(prefix + "UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable(prefix + "UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable(prefix + "RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable(prefix + "UserTokens");

        // Custom tables
        builder.Entity<ProfileEntity>().ToTable(prefix + "Profiles");
        builder.Entity<AddressEntity>().ToTable(prefix + "Addresses");
        builder.Entity<EmailVfTokenEntity>().ToTable(prefix + "EmailVerificationTokens");

        builder.Entity<ProfileEntity>()
               .HasOne(p => p.User)
               .WithOne(u => u.Profile)
               .HasForeignKey<ProfileEntity>(p => p.UserId);

        builder.Entity<AddressEntity>()
               .ToTable(prefix + "Addresses")
               .HasOne(a => a.Profile)
               .WithMany(p => p.Addresses)
               .HasForeignKey(a => a.ProfileId);

        builder.Entity<EmailVfTokenEntity>()
               .HasOne(t => t.User)
               .WithMany()
               .HasForeignKey(t => t.UserId);
    }
}
