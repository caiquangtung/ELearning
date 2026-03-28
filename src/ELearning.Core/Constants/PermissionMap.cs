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
                set.Add(Permissions.Admin.Access);
                set.Add(Permissions.Organizations.Read);
                set.Add(Permissions.Organizations.Manage);
                continue;
            }

            if (r.Equals(Roles.OrgAdmin, StringComparison.OrdinalIgnoreCase))
            {
                set.Add(Permissions.Organizations.Read);
                set.Add(Permissions.Organizations.Manage);
                continue;
            }

            if (r.Equals(Roles.Instructor, StringComparison.OrdinalIgnoreCase))
            {
                set.Add(Permissions.Organizations.Read);
            }
        }

        return set;
    }
}
