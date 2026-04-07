namespace ELearning.Core.Constants;

public static class PermissionMap
{
    public static IReadOnlySet<string> GetPermissionsForRoles(IEnumerable<string> roleNames)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var r in roleNames)
        {
            if (r.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var p in Permissions.All)
                    set.Add(p);
                continue;
            }

            if (r.Equals(Roles.OrgAdmin, StringComparison.OrdinalIgnoreCase))
            {
                set.Add(Permissions.Users.Read);
                set.Add(Permissions.Users.Create);
                set.Add(Permissions.Users.Update);
                set.Add(Permissions.Organizations.Read);
                set.Add(Permissions.Organizations.Manage);
                set.Add(Permissions.Courses.Read);
                set.Add(Permissions.Classes.Read);
                set.Add(Permissions.Classes.Create);
                set.Add(Permissions.Classes.Update);
                set.Add(Permissions.Classes.ManageSessions);
                set.Add(Permissions.Enrollments.Read);
                set.Add(Permissions.Enrollments.Create);
                set.Add(Permissions.Enrollments.Manage);
                set.Add(Permissions.Licenses.Read);
                set.Add(Permissions.Licenses.Assign);
                set.Add(Permissions.Reports.Read);
                set.Add(Permissions.Reports.Export);
                continue;
            }

            if (r.Equals(Roles.Instructor, StringComparison.OrdinalIgnoreCase))
            {
                set.Add(Permissions.Courses.Read);
                set.Add(Permissions.Courses.Create);
                set.Add(Permissions.Courses.Update);
                set.Add(Permissions.Courses.Publish);
                set.Add(Permissions.Classes.Read);
                set.Add(Permissions.Classes.Create);
                set.Add(Permissions.Classes.Update);
                set.Add(Permissions.Classes.ManageSessions);
                set.Add(Permissions.Enrollments.Read);
                set.Add(Permissions.Reports.Read);
                continue;
            }

            if (r.Equals(Roles.Learner, StringComparison.OrdinalIgnoreCase))
            {
                set.Add(Permissions.Courses.Read);
                set.Add(Permissions.Classes.Read);
                set.Add(Permissions.Enrollments.Read);
                set.Add(Permissions.Enrollments.Create);
            }
        }

        return set;
    }
}
