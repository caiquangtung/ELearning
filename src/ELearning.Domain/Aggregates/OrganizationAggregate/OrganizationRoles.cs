namespace ELearning.Domain.Aggregates.OrganizationAggregate;

public static class OrganizationRoles
{
    public const string OrgAdmin = "OrgAdmin";
    public const string Member = "Member";
    public const string Instructor = "Instructor";

    public static readonly IReadOnlyList<string> All = [OrgAdmin, Member, Instructor];

    public static bool IsValid(string role) => All.Contains(role, StringComparer.OrdinalIgnoreCase);
}
