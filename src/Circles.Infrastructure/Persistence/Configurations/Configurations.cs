using Circles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circles.Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
    {
        b.ToTable("persons");
        b.HasKey(p => p.Id);
        b.Property(p => p.FirstName).HasMaxLength(200).IsRequired();
        b.Property(p => p.LastName).HasMaxLength(200).IsRequired();

        // A Person may have zero or one UserAccount. The FK lives on UserAccount.
        b.HasOne(p => p.UserAccount)
            .WithOne(u => u.Person)
            .HasForeignKey<UserAccount>(u => u.PersonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> b)
    {
        b.ToTable("user_accounts");
        b.HasKey(u => u.Id);
        b.Property(u => u.Email).HasMaxLength(320).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.PasswordHash).IsRequired();
    }
}

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> b)
    {
        b.ToTable("relationships");
        b.HasKey(r => r.Id);
        b.Property(r => r.Type).HasConversion<string>().HasMaxLength(50);

        b.HasOne(r => r.FromPerson)
            .WithMany(p => p.OutgoingRelationships)
            .HasForeignKey(r => r.FromPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.ToPerson)
            .WithMany(p => p.IncomingRelationships)
            .HasForeignKey(r => r.ToPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => new { r.FromPersonId, r.Type });
        b.HasIndex(r => new { r.ToPersonId, r.Type });
    }
}

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.ToTable("organizations");
        b.HasKey(o => o.Id);
        b.Property(o => o.Name).HasMaxLength(300).IsRequired();
        b.Property(o => o.Slug).HasMaxLength(120).IsRequired();
        b.HasIndex(o => o.Slug).IsUnique();
    }
}

public class CircleConfiguration : IEntityTypeConfiguration<Circle>
{
    public void Configure(EntityTypeBuilder<Circle> b)
    {
        b.ToTable("circles");
        b.HasKey(c => c.Id);
        b.Property(c => c.Name).HasMaxLength(300).IsRequired();
        b.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        b.Property(c => c.Type).HasConversion<string>().HasMaxLength(50);

        b.HasOne(c => c.Organization)
            .WithMany(o => o.Circles)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(c => c.ParentCircle)
            .WithMany(c => c.ChildCircles)
            .HasForeignKey(c => c.ParentCircleId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(c => new { c.OrganizationId, c.Slug }).IsUnique();
    }
}

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> b)
    {
        b.ToTable("memberships");
        b.HasKey(m => m.Id);
        b.Property(m => m.Role).HasConversion<string>().HasMaxLength(50);

        b.HasOne(m => m.Person)
            .WithMany(p => p.Memberships)
            .HasForeignKey(m => m.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(m => m.Circle)
            .WithMany(c => c.Memberships)
            .HasForeignKey(m => m.CircleId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(m => new { m.CircleId, m.ValidUntil });
        b.HasIndex(m => new { m.PersonId, m.ValidUntil });
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("role_permissions");
        b.HasKey(rp => rp.Id);
        b.Property(rp => rp.Role).HasConversion<string>().HasMaxLength(50);
        b.Property(rp => rp.Permission).HasConversion<string>().HasMaxLength(50);
        b.HasIndex(rp => new { rp.Role, rp.Permission }).IsUnique();
    }
}

public class MagicLinkTokenConfiguration : IEntityTypeConfiguration<MagicLinkToken>
{
    public void Configure(EntityTypeBuilder<MagicLinkToken> b)
    {
        b.ToTable("magic_link_tokens");
        b.HasKey(t => t.Id);
        b.Property(t => t.Token).HasMaxLength(128).IsRequired();
        b.HasIndex(t => t.Token).IsUnique();

        b.HasOne(t => t.UserAccount)
            .WithMany()
            .HasForeignKey(t => t.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(t => t.UserAccountId);
    }
}
