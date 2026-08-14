using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spracher.IdentityModel;

namespace Spracher.Persistence;

public sealed class SpracherDbContext(
    DbContextOptions<SpracherDbContext> options,
    IEnumerable<IDbModelConfigurator> modelConfigurators)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");
        ConfigureIdentityModel(builder);

        foreach (var configurator in modelConfigurators)
        {
            configurator.Configure(builder);
        }
    }

    private static void ConfigureIdentityModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users", "iam");
            entity.Property(user => user.DisplayName).HasMaxLength(80).IsRequired();
            entity.Property(user => user.TimeZoneId).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.Property(user => user.ProfileUpdatedAt);
            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("UX_Users_NormalizedEmail");
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles", "iam");
            entity.HasData(SystemRoles.All.Select(role => new ApplicationRole
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.Name.ToUpperInvariant(),
                ConcurrencyStamp = role.ConcurrencyStamp,
            }));
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "iam");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "iam");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "iam");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "iam");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "iam");
    }
}
