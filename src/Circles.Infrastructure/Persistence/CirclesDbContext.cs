using Circles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Circles.Infrastructure.Persistence;

/// <summary>
/// The EF Core context for the Circles modular monolith. All bounded concepts
/// (people, accounts, memberships, relationships, organizations, circles and the
/// role/permission model) live in one database but are kept as distinct sets.
/// </summary>
public class CirclesDbContext : DbContext
{
    public CirclesDbContext(DbContextOptions<CirclesDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Circle> Circles => Set<Circle>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CirclesDbContext).Assembly);
    }
}
