namespace ELearning.Core.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string Read = "Users.Read";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    public static class Courses
    {
        public const string Read = "Courses.Read";
        public const string Create = "Courses.Create";
        public const string Update = "Courses.Update";
        public const string Delete = "Courses.Delete";
        public const string Publish = "Courses.Publish";
    }

    public static class Organizations
    {
        public const string Read = "Organizations.Read";
        public const string Manage = "Organizations.Manage";
    }

    public static class Enrollments
    {
        public const string Read = "Enrollments.Read";
        public const string Create = "Enrollments.Create";
        public const string Manage = "Enrollments.Manage";
    }

    public static class Licenses
    {
        public const string Read = "Licenses.Read";
        public const string Assign = "Licenses.Assign";
    }

    public static class Reports
    {
        public const string Read = "Reports.Read";
        public const string Export = "Reports.Export";
    }

    public static class Admin
    {
        public const string Access = "Admin.Access";
    }

    public static IReadOnlyList<string> All =>
    [
        Users.Read, Users.Create, Users.Update, Users.Delete,
        Courses.Read, Courses.Create, Courses.Update, Courses.Delete, Courses.Publish,
        Organizations.Read, Organizations.Manage,
        Enrollments.Read, Enrollments.Create, Enrollments.Manage,
        Licenses.Read, Licenses.Assign,
        Reports.Read, Reports.Export,
        Admin.Access
    ];
}
