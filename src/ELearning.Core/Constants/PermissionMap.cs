namespace ELearning.Core.Constants;

public static class PermissionMap
{
    private static readonly Dictionary<string, IReadOnlyList<string>> _map = new()
    {
        [Roles.Admin] =
        [
            ..Permissions.All
        ],

        [Roles.OrgAdmin] =
        [
            Permissions.Users.Read,
            Permissions.Users.Create,
            Permissions.Users.Update,
            Permissions.Organizations.Read,
            Permissions.Organizations.Manage,
            Permissions.Courses.Read,
            Permissions.Enrollments.Read,
            Permissions.Enrollments.Create,
            Permissions.Enrollments.Manage,
            Permissions.Licenses.Read,
            Permissions.Licenses.Assign,
            Permissions.Reports.Read,
            Permissions.Reports.Export
        ],

        [Roles.Instructor] =
        [
            Permissions.Courses.Read,
            Permissions.Courses.Create,
            Permissions.Courses.Update,
            Permissions.Courses.Publish,
            Permissions.Enrollments.Read,
            Permissions.Reports.Read
        ],

        [Roles.Learner] =
        [
            Permissions.Courses.Read,
            Permissions.Enrollments.Read,
            Permissions.Enrollments.Create
        ]
    };

    public static IReadOnlyList<string> GetPermissions(string role) =>
        _map.TryGetValue(role, out var perms) ? perms : [];

    public static IReadOnlyList<string> GetPermissionsForRoles(IEnumerable<string> roles) =>
        roles.SelectMany(GetPermissions).Distinct().ToList();
}
