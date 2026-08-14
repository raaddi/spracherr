namespace Spracher.IdentityModel;

public static class SystemRoles
{
    public const string SelfLearner = "SelfLearner";
    public const string Student = "Student";
    public const string Teacher = "Teacher";
    public const string Admin = "Admin";
    public const string SchoolAdmin = "SchoolAdmin";

    public static IReadOnlyList<SystemRoleDefinition> All { get; } =
    [
        new(
            Guid.Parse("0198ac40-0000-7000-8000-000000000001"),
            SelfLearner,
            "role-self-learner-v1"),
        new(
            Guid.Parse("0198ac40-0000-7000-8000-000000000002"),
            Student,
            "role-student-v1"),
        new(
            Guid.Parse("0198ac40-0000-7000-8000-000000000003"),
            Teacher,
            "role-teacher-v1"),
        new(
            Guid.Parse("0198ac40-0000-7000-8000-000000000004"),
            Admin,
            "role-admin-v1"),
        new(
            Guid.Parse("0198ac40-0000-7000-8000-000000000005"),
            SchoolAdmin,
            "role-school-admin-v1"),
    ];
}

public sealed record SystemRoleDefinition(
    Guid Id,
    string Name,
    string ConcurrencyStamp);
