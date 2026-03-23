namespace ELearning.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string OrgAdmin = "OrgAdmin";
    public const string Instructor = "Instructor";
    public const string Learner = "Learner";

    public static readonly IReadOnlyList<string> All = [Admin, OrgAdmin, Instructor, Learner];

    public static bool IsValid(string role) => All.Contains(role, StringComparer.OrdinalIgnoreCase);
}
