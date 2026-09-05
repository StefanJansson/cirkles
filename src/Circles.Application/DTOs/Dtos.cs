using Circles.Domain.Enums;

namespace Circles.Application.DTOs;

public record PersonDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    bool HasUserAccount,
    string? Email);

public record CircleAccessDto(
    Guid CircleId,
    string Name,
    string Slug,
    CircleType Type,
    Guid? ParentCircleId,
    string AccessKind); // "Direct" or "Derived"

public record OrganizationDto(
    Guid Id,
    string Name,
    string Slug);

public record CircleNodeDto(
    Guid Id,
    string Name,
    string Slug,
    CircleType Type,
    Guid? ParentCircleId,
    List<CircleNodeDto> Children);

public record MemberDto(
    Guid MembershipId,
    Guid PersonId,
    string FullName,
    MembershipRole Role,
    DateTime ValidFrom,
    DateTime? ValidUntil);

public record PermissionsDto(
    Guid PersonId,
    Guid CircleId,
    bool CanAccess,
    List<PermissionType> Permissions);
