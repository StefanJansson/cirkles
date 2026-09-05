using Circles.Domain.Entities;
using Circles.Domain.Enums;
using Circles.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Circles.Infrastructure.Seeding;

/// <summary>
/// Seeds the database with realistic Swedish demo data for the fictional club
/// "Uppsala IK", plus the role → permission mapping. Deterministic GUIDs are used
/// so the seed is idempotent and stable across runs.
/// </summary>
public static class DataSeeder
{
    private static Guid Id(string key) =>
        new Guid(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(key)));

    // Fixed reference "now" so ValidFrom dates are stable and clearly in the past.
    private static readonly DateTime Now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Shared password for every demo account so the prototype is easy to log in
    // to. NOT a real credential policy — real accounts set their own password
    // during onboarding. Hashed with BCrypt at seed time.
    public const string DemoPassword = "Cirkles123!";
    private static readonly string DemoPasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);

    public static async Task SeedAsync(CirclesDbContext db)
    {
        await SeedRolePermissionsAsync(db);
        await SeedUppsalaIkAsync(db);
    }

    private static async Task SeedRolePermissionsAsync(CirclesDbContext db)
    {
        if (await db.RolePermissions.AnyAsync()) return;

        foreach (var (role, permissions) in RolePermissionMap.Map)
        {
            foreach (var permission in permissions)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    Id = Id($"rp:{role}:{permission}"),
                    Role = role,
                    Permission = permission
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedUppsalaIkAsync(CirclesDbContext db)
    {
        if (await db.Organizations.AnyAsync()) return;

        // ---- Organization -------------------------------------------------
        var org = new Organization
        {
            Id = Id("org:uppsala-ik"),
            Name = "Uppsala IK",
            Slug = "uppsala-ik",
            CreatedAt = Now
        };
        db.Organizations.Add(org);

        // ---- Circles ------------------------------------------------------
        var root = new Circle
        {
            Id = Id("circle:uppsala-ik"),
            OrganizationId = org.Id,
            ParentCircleId = null,
            Name = "Uppsala IK",
            Slug = "uppsala-ik",
            Type = CircleType.General,
            CreatedAt = Now
        };
        var p2016 = new Circle
        {
            Id = Id("circle:p2016"),
            OrganizationId = org.Id,
            ParentCircleId = root.Id,
            Name = "P2016",
            Slug = "p2016",
            Type = CircleType.Team,
            CreatedAt = Now
        };
        var p2014 = new Circle
        {
            Id = Id("circle:p2014"),
            OrganizationId = org.Id,
            ParentCircleId = root.Id,
            Name = "P2014",
            Slug = "p2014",
            Type = CircleType.Team,
            CreatedAt = Now
        };
        var f2016 = new Circle
        {
            Id = Id("circle:f2016"),
            OrganizationId = org.Id,
            ParentCircleId = root.Id,
            Name = "F2016",
            Slug = "f2016",
            Type = CircleType.Team,
            CreatedAt = Now
        };
        var board = new Circle
        {
            Id = Id("circle:styrelsen"),
            OrganizationId = org.Id,
            ParentCircleId = root.Id,
            Name = "Styrelsen",
            Slug = "styrelsen",
            Type = CircleType.Board,
            CreatedAt = Now
        };
        var officials = new Circle
        {
            Id = Id("circle:funktionarer"),
            OrganizationId = org.Id,
            ParentCircleId = root.Id,
            Name = "Funktionärer",
            Slug = "funktionarer",
            Type = CircleType.General,
            CreatedAt = Now
        };
        db.Circles.AddRange(root, p2016, p2014, f2016, board, officials);

        // ---- People (some with accounts, some without) --------------------
        // Johan Andersson — has a UserAccount, guardian of Alexander.
        var johan = NewPerson("johan", "Johan", "Andersson");
        // Alexander Andersson — a 10-year-old child with NO UserAccount.
        var alexander = NewPerson("alexander", "Alexander", "Andersson");
        // Lisa Berg — a child with NO UserAccount.
        var lisa = NewPerson("lisa", "Lisa", "Berg");
        // Anna Berg — has a UserAccount, guardian of Lisa.
        var anna = NewPerson("anna", "Anna", "Berg");
        // Erik Svensson — has a UserAccount, coach.
        var erik = NewPerson("erik", "Erik", "Svensson");
        // Maria Lindgren — has a UserAccount, club administrator.
        var maria = NewPerson("maria", "Maria", "Lindgren");
        db.Persons.AddRange(johan, alexander, lisa, anna, erik, maria);

        db.UserAccounts.AddRange(
            NewAccount("johan", "johan@example.com", johan.Id),
            NewAccount("anna", "anna@example.com", anna.Id),
            NewAccount("erik", "erik@example.com", erik.Id),
            NewAccount("maria", "maria@example.com", maria.Id)
        );
        // NOTE: Alexander and Lisa deliberately have NO UserAccount.

        // ---- Relationships (explicit, time-based) -------------------------
        db.Relationships.AddRange(
            NewRelationship("johan-guardian-alexander", johan.Id, alexander.Id, RelationshipType.GuardianOf),
            NewRelationship("anna-guardian-lisa", anna.Id, lisa.Id, RelationshipType.GuardianOf)
        );

        // ---- Memberships (time-based, never deleted) ----------------------
        db.Memberships.AddRange(
            NewMembership("alexander-p2016", alexander.Id, p2016.Id, MembershipRole.Player),
            NewMembership("lisa-f2016", lisa.Id, f2016.Id, MembershipRole.Player),
            NewMembership("erik-p2016", erik.Id, p2016.Id, MembershipRole.Coach),
            NewMembership("maria-root", maria.Id, root.Id, MembershipRole.Administrator),
            NewMembership("johan-officials", johan.Id, officials.Id, MembershipRole.Member)
        );
        // NOTE: Johan has NO direct membership in P2016. His access to P2016 is
        // DERIVED from being guardian of Alexander, who plays in P2016.

        await db.SaveChangesAsync();
    }

    private static Person NewPerson(string key, string first, string last) => new()
    {
        Id = Id($"person:{key}"),
        FirstName = first,
        LastName = last,
        CreatedAt = Now
    };

    private static UserAccount NewAccount(string key, string email, Guid personId) => new()
    {
        Id = Id($"account:{key}"),
        Email = email,
        PasswordHash = DemoPasswordHash,
        PersonId = personId,
        CreatedAt = Now
    };

    private static Relationship NewRelationship(string key, Guid from, Guid to, RelationshipType type) => new()
    {
        Id = Id($"rel:{key}"),
        FromPersonId = from,
        ToPersonId = to,
        Type = type,
        ValidFrom = Now.AddYears(-2),
        ValidUntil = null
    };

    private static Membership NewMembership(string key, Guid personId, Guid circleId, MembershipRole role) => new()
    {
        Id = Id($"mem:{key}"),
        PersonId = personId,
        CircleId = circleId,
        Role = role,
        ValidFrom = Now.AddYears(-1),
        ValidUntil = null
    };
}
