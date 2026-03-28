namespace ELearning.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string OrgAdmin = "OrgAdmin";
    public const string Instructor = "Instructor";
    public const string Learner = "Learner";

    private static readonly HashSet<string> Valid =
    [
        Admin,
        OrgAdmin,
        Instructor,
        Learner
    ];

    public static bool IsValid(string role) => Valid.Contains(role);
}
