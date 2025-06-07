
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Presentation.Models;

namespace Presentaion.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; } = null!;
    public DbSet<EmailVfTokenEntity> EmailVerificationTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        const string prefix = "auth_";

        builder.Entity<ApplicationUser>().ToTable(prefix + "Users");
        builder.Entity<ApplicationRole>().ToTable(prefix + "Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable(prefix + "UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable(prefix + "UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable(prefix + "UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable(prefix + "RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable(prefix + "UserTokens");

        builder.Entity<EmailVfTokenEntity>(b =>
        {
            b.ToTable(prefix + "EmailVerificationTokens");
            b.HasKey(t => t.Id);
            b.Property(t => t.Code).IsRequired();
            b.Property(t => t.ExpiresAtUtc).IsRequired();
            b.Property(t => t.Used).IsRequired();
            b.HasOne(t => t.User)
             .WithMany()
             .HasForeignKey(t => t.UserId);
        });

        builder.Entity<RefreshTokenEntity>(b =>
        {
            b.ToTable(prefix + "RefreshTokens");
            b.HasKey(r => r.Id);
            b.Property(r => r.Token).IsRequired();
            b.HasIndex(r => r.Token).IsUnique();
            b.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId);
        });
    }
}
