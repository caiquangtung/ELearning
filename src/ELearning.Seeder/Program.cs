using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Aggregates.NotificationAggregate;
using ELearning.Domain.Aggregates.OrganizationAggregate;
using ELearning.Domain.Aggregates.QuizAggregate;
using ELearning.Domain.Aggregates.ReviewAggregate;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Domain.Aggregates.UserAggregate;
using ELearning.Domain.Aggregates.VideoAggregate;
using ELearning.Infrastructure.Identity;
using ELearning.Infrastructure.Persistence;
using ELearning.Seeder;
using Microsoft.EntityFrameworkCore;
using static SeedText;

var options = SeederOptions.Parse(args);
var profile = SeedProfile.Get(options.Profile);

Console.WriteLine($"ELearning seeder profile={profile.Name}, prefix={options.Prefix}, batch={options.BatchSize}");

await using var db = CreateDbContext(options.ConnectionString);
await db.Database.MigrateAsync();

if (options.ResetPrefix)
{
    Console.WriteLine("Resetting existing seed data for prefix...");
    await ResetPrefixAsync(db, options.Prefix);
}

var emailPrefix = $"{options.Prefix}.";
var alreadySeeded = await db.Users.AnyAsync(u => u.Email.StartsWith(emailPrefix));
if (alreadySeeded && !options.Append)
{
    Console.WriteLine("Seed data already exists. Use --append or --reset-prefix to run again.");
    return;
}

db.ChangeTracker.AutoDetectChangesEnabled = false;

var hasher = new BcryptPasswordHasher();
var sharedPasswordHash = hasher.Hash(options.Password);

var users = await SeedUsersAsync(db, profile, options, sharedPasswordHash);
var orgAdmins = users.Where(u => u.HasRole(Roles.OrgAdmin)).Select(u => u.Id).ToArray();
var instructors = users.Where(u => u.HasRole(Roles.Instructor)).Select(u => u.Id).ToArray();
var learners = users.Where(u => u.HasRole(Roles.Learner)).Select(u => u.Id).ToArray();

await SeedOrganizationsAsync(db, profile, options, orgAdmins, instructors, learners);

var courses = await SeedCoursesAsync(db, profile, options);
await SeedTrainingClassesAsync(db, profile, options, courses.Select(c => c.Id).ToArray(), instructors);
await SeedQuizzesAsync(db, profile, options, courses);
await SeedReviewsAsync(db, profile, options, courses.Select(c => c.Id).ToArray(), learners, orgAdmins);
await SeedNotificationsAsync(db, profile, options, learners);

var videos = await SeedVideoAssetsAsync(db, profile, options, courses);
await SeedWatchEventsAsync(db, profile, options, videos, learners);

Console.WriteLine("Seed complete:");
Console.WriteLine($"  users={users.Count}");
Console.WriteLine($"  organizations={profile.OrganizationCount}");
Console.WriteLine($"  courses={courses.Count}");
Console.WriteLine($"  classes={profile.TrainingClassCount}");
Console.WriteLine($"  quizzes={Math.Min(profile.QuizCount, courses.Count)}");
Console.WriteLine($"  notifications={profile.NotificationCount}");
Console.WriteLine($"  videos={videos.Count}");
Console.WriteLine($"  watchEvents<={profile.WatchEventCount}");
Console.WriteLine($"Default password for seeded users: {options.Password}");

static ApplicationDbContext CreateDbContext(string connectionString)
{
    var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(connectionString);

    return new ApplicationDbContext(builder.Options, new SystemCurrentUserService());
}

static async Task<List<User>> SeedUsersAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    string passwordHash)
{
    var users = new List<User>(profile.UserCount);
    var orgAdminCount = Math.Max(1, profile.UserCount / 25);
    var instructorCount = Math.Max(2, profile.UserCount / 10);
    var learnerCount = profile.UserCount - orgAdminCount - instructorCount;

    for (var i = 1; i <= orgAdminCount; i++)
    {
        var user = User.Create($"{options.Prefix}.orgadmin{i:D5}@seed.local", passwordHash, "Org", $"Admin {i:D5}", Roles.OrgAdmin);
        users.Add(user);
    }

    for (var i = 1; i <= instructorCount; i++)
    {
        var user = User.Create($"{options.Prefix}.instructor{i:D5}@seed.local", passwordHash, "Instructor", $"{i:D5}", Roles.Instructor);
        users.Add(user);
    }

    for (var i = 1; i <= learnerCount; i++)
    {
        var user = User.Create($"{options.Prefix}.learner{i:D5}@seed.local", passwordHash, FirstNames[i % FirstNames.Length], LastNames[i % LastNames.Length], Roles.Learner);
        users.Add(user);
    }

    await AddInBatchesAsync(db, users, options.BatchSize);
    Console.WriteLine($"Seeded users: {users.Count}");
    return users;
}

static async Task SeedOrganizationsAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    IReadOnlyList<Guid> orgAdmins,
    IReadOnlyList<Guid> instructors,
    IReadOnlyList<Guid> learners)
{
    var organizations = new List<Organization>(profile.OrganizationCount);

    for (var i = 1; i <= profile.OrganizationCount; i++)
    {
        var org = Organization.Create($"{options.Prefix.ToUpperInvariant()} Organization {i:D4}", $"{options.Prefix}-org-{i:D4}");
        var engineering = org.AddDepartment("Engineering", null);
        var learning = org.AddDepartment("Learning Operations", null);
        var sales = org.AddDepartment("Sales Enablement", null);

        org.AddMember(orgAdmins[(i - 1) % orgAdmins.Count], learning.Id, OrganizationRoles.OrgAdmin);
        org.AddMember(instructors[(i - 1) % instructors.Count], engineering.Id, OrganizationRoles.Instructor);

        var members = Math.Min(profile.MembersPerOrganization, learners.Count);
        for (var j = 0; j < members; j++)
        {
            var learnerId = learners[((i - 1) * members + j) % learners.Count];
            var departmentId = j % 3 == 0 ? engineering.Id : j % 3 == 1 ? learning.Id : sales.Id;
            if (org.Members.All(m => m.UserId != learnerId))
                org.AddMember(learnerId, departmentId, OrganizationRoles.Member);
        }

        organizations.Add(org);
    }

    await AddInBatchesAsync(db, organizations, options.BatchSize);
    Console.WriteLine($"Seeded organizations: {organizations.Count}");
}

static async Task<List<Course>> SeedCoursesAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options)
{
    var courses = new List<Course>(profile.CourseCount);

    for (var i = 1; i <= profile.CourseCount; i++)
    {
        var topic = CourseTopics[i % CourseTopics.Length];
        var course = Course.Create(
            $"[{options.Prefix}] {topic} Fundamentals {i:D5}",
            $"Generated seed course for {topic}. Contains structured lessons, quizzes, reviews, classes, and watch progress data.");
        course.SetPrice((i % 12) * 10_00, "USD");

        for (var sectionIndex = 1; sectionIndex <= profile.SectionsPerCourse; sectionIndex++)
        {
            var section = course.AddSection($"Module {sectionIndex}: {ModuleTitles[(i + sectionIndex) % ModuleTitles.Length]}");
            for (var lessonIndex = 1; lessonIndex <= profile.LessonsPerSection; lessonIndex++)
            {
                var lesson = section.AddLesson($"Lesson {lessonIndex}: {LessonTitles[(i + lessonIndex) % LessonTitles.Length]}");
                lesson.UpdateContent($"Seed content for {course.Title}, section {sectionIndex}, lesson {lessonIndex}.");
            }
        }

        course.Publish();
        courses.Add(course);
    }

    await AddInBatchesAsync(db, courses, options.BatchSize);
    Console.WriteLine($"Seeded courses: {courses.Count}");
    return courses;
}

static async Task SeedTrainingClassesAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    IReadOnlyList<Guid> courseIds,
    IReadOnlyList<Guid> instructors)
{
    var classes = new List<TrainingClass>(profile.TrainingClassCount);
    var startBase = DateTime.UtcNow.Date.AddDays(7);

    for (var i = 1; i <= profile.TrainingClassCount; i++)
    {
        var courseId = courseIds[(i - 1) % courseIds.Count];
        var trainingClass = TrainingClass.Create(courseId, $"[{options.Prefix}] Cohort {i:D5}", 20 + i % 80);
        trainingClass.SetPrice((i % 10) * 15_00, "USD");
        trainingClass.AddInstructor(instructors[(i - 1) % instructors.Count]);

        for (var sessionIndex = 1; sessionIndex <= profile.SessionsPerClass; sessionIndex++)
        {
            var start = startBase.AddDays(i % 45).AddHours(2 * sessionIndex);
            trainingClass.ScheduleSession(
                $"Session {sessionIndex}",
                sessionIndex % 3 == 0 ? ClassSessionType.Offline : ClassSessionType.Zoom,
                start,
                start.AddHours(2),
                sessionIndex % 3 == 0 ? $"Room {100 + i % 30}" : null,
                sessionIndex % 3 == 0 ? null : $"seed-{options.Prefix}-{i:D5}-{sessionIndex}",
                sessionIndex % 3 == 0 ? null : $"https://zoom.local/{options.Prefix}/{i:D5}/{sessionIndex}");
        }

        classes.Add(trainingClass);
    }

    await AddInBatchesAsync(db, classes, options.BatchSize);
    Console.WriteLine($"Seeded training classes: {classes.Count}");
}

static async Task SeedQuizzesAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    IReadOnlyList<Course> courses)
{
    var quizCount = Math.Min(profile.QuizCount, courses.Count);
    var quizzes = new List<Quiz>(quizCount);

    for (var i = 0; i < quizCount; i++)
    {
        var course = courses[i];
        var quiz = Quiz.CreateForCourse(course.Id, $"[{options.Prefix}] Knowledge Check {i + 1:D5}", "Generated quiz for seed data.", 30, 70);

        for (var q = 1; q <= profile.QuestionsPerQuiz; q++)
        {
            var question = quiz.AddQuestion($"Question {q}: What is the best answer for scenario {i + q}?", QuestionType.MultipleChoice, 5, q);
            question.AddOption("Apply the recommended learning workflow.", true, 1);
            question.AddOption("Skip validation and publish immediately.", false, 2);
            question.AddOption("Ignore learner progress data.", false, 3);
            question.AddOption("Remove all review checkpoints.", false, 4);
        }

        quiz.Publish();
        quizzes.Add(quiz);
    }

    await AddInBatchesAsync(db, quizzes, options.BatchSize);
    Console.WriteLine($"Seeded quizzes: {quizzes.Count}");
}

static async Task SeedReviewsAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    IReadOnlyList<Guid> courseIds,
    IReadOnlyList<Guid> learners,
    IReadOnlyList<Guid> moderators)
{
    var reviews = new List<Review>(Math.Min(profile.ReviewCount, courseIds.Count * learners.Count));

    for (var i = 0; i < courseIds.Count && reviews.Count < profile.ReviewCount; i++)
    {
        var perCourse = Math.Min(profile.ReviewsPerCourse, learners.Count);
        for (var j = 0; j < perCourse && reviews.Count < profile.ReviewCount; j++)
        {
            var learnerId = learners[(i * 31 + j) % learners.Count];
            var review = Review.Submit(courseIds[i], learnerId, 3 + (i + j) % 3, $"Seed review {reviews.Count + 1} for {options.Prefix} course data.");
            if ((i + j) % 5 != 0)
                review.Approve(moderators[(i + j) % moderators.Count]);
            reviews.Add(review);
        }
    }

    await AddInBatchesAsync(db, reviews, options.BatchSize);
    Console.WriteLine($"Seeded reviews: {reviews.Count}");
}

static async Task SeedNotificationsAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    IReadOnlyList<Guid> learners)
{
    var notifications = new List<Notification>(Math.Min(options.BatchSize, profile.NotificationCount));

    for (var i = 1; i <= profile.NotificationCount; i++)
    {
        var notification = Notification.Create(
            learners[(i - 1) % learners.Count],
            $"[{options.Prefix}] Learning reminder {i:D6}",
            "This is generated notification data for dashboard, unread count, and pagination tests.",
            i % 7 == 0 ? NotificationType.Warning : i % 3 == 0 ? NotificationType.Reminder : NotificationType.Info,
            i % 4 == 0 ? "/courses" : null);

        if (i % 4 == 0)
            notification.MarkAsRead(DateTime.UtcNow);

        notifications.Add(notification);

        if (notifications.Count == options.BatchSize)
        {
            await AddInBatchesAsync(db, notifications, options.BatchSize);
            notifications.Clear();
        }
    }

    if (notifications.Count > 0)
        await AddInBatchesAsync(db, notifications, options.BatchSize);

    Console.WriteLine($"Seeded notifications: {profile.NotificationCount}");
}

static async Task<List<VideoAsset>> SeedVideoAssetsAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    IReadOnlyList<Course> courses)
{
    var lessonIds = courses
        .SelectMany(c => c.Sections)
        .SelectMany(s => s.Lessons)
        .Select(l => l.Id)
        .Take(profile.VideoAssetCount)
        .ToArray();

    var videos = new List<VideoAsset>(lessonIds.Length);
    for (var i = 0; i < lessonIds.Length; i++)
    {
        videos.Add(VideoAsset.Create(
            lessonIds[i],
            $"seed-video-{options.Prefix}-{i + 1:D6}.mp4",
            "video/mp4",
            50_000_000 + i * 1024,
            $"seed/{options.Prefix}/videos/{i + 1:D6}.mp4",
            $"/storage/seed/{options.Prefix}/videos/{i + 1:D6}.mp4",
            600 + i % 1800));
    }

    await AddInBatchesAsync(db, videos, options.BatchSize);
    Console.WriteLine($"Seeded video assets: {videos.Count}");
    return videos;
}

static async Task SeedWatchEventsAsync(
    ApplicationDbContext db,
    SeedProfile profile,
    SeederOptions options,
    IReadOnlyList<VideoAsset> videos,
    IReadOnlyList<Guid> learners)
{
    if (videos.Count == 0 || learners.Count == 0 || profile.WatchEventCount == 0)
        return;

    var maxEvents = Math.Min(profile.WatchEventCount, videos.Count * learners.Count);
    var events = new List<WatchEvent>(Math.Min(options.BatchSize, maxEvents));
    var created = 0;

    for (var videoIndex = 0; videoIndex < videos.Count && created < maxEvents; videoIndex++)
    {
        var video = videos[videoIndex];
        for (var learnerIndex = 0; learnerIndex < learners.Count && created < maxEvents; learnerIndex++)
        {
            var duration = video.DurationSeconds ?? 900;
            var watched = 60 + (created * 37) % duration;
            var watch = WatchEvent.Start(video.Id, video.LessonId, learners[(learnerIndex + videoIndex) % learners.Count]);
            watch.RecordProgress(watched, duration, watched, DateTime.UtcNow);
            events.Add(watch);
            created++;

            if (events.Count == options.BatchSize)
            {
                await AddInBatchesAsync(db, events, options.BatchSize);
                events.Clear();
            }
        }
    }

    if (events.Count > 0)
        await AddInBatchesAsync(db, events, options.BatchSize);

    Console.WriteLine($"Seeded watch events: {created}");
}

static async Task AddInBatchesAsync<TEntity>(
    ApplicationDbContext db,
    IReadOnlyList<TEntity> entities,
    int batchSize)
    where TEntity : class
{
    for (var offset = 0; offset < entities.Count; offset += batchSize)
    {
        var batch = entities.Skip(offset).Take(batchSize).ToArray();
        await db.AddRangeAsync(batch);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }
}

static async Task ResetPrefixAsync(ApplicationDbContext db, string prefix)
{
    var emailPrefix = $"{prefix}.";
    var titlePrefix = $"[{prefix}]";
    var slugPrefix = $"{prefix}-org-";
    var storagePrefix = $"seed/{prefix}/";

    var userIds = await db.Users
        .Where(u => u.Email.StartsWith(emailPrefix))
        .Select(u => u.Id)
        .ToArrayAsync();

    var courseIds = await db.Courses
        .IgnoreQueryFilters()
        .Where(c => c.Title.StartsWith(titlePrefix))
        .Select(c => c.Id)
        .ToArrayAsync();

    var sectionIds = await db.Set<Section>()
        .IgnoreQueryFilters()
        .Where(s => courseIds.Contains(s.CourseId))
        .Select(s => s.Id)
        .ToArrayAsync();

    var lessonIds = await db.Set<Lesson>()
        .IgnoreQueryFilters()
        .Where(l => sectionIds.Contains(l.SectionId))
        .Select(l => l.Id)
        .ToArrayAsync();

    var videoIds = await db.VideoAssets
        .Where(v => v.StorageKey.StartsWith(storagePrefix) || lessonIds.Contains(v.LessonId))
        .Select(v => v.Id)
        .ToArrayAsync();

    await db.WatchEvents
        .Where(w => userIds.Contains(w.UserId) || videoIds.Contains(w.VideoAssetId))
        .ExecuteDeleteAsync();

    await db.VideoAssets
        .Where(v => videoIds.Contains(v.Id))
        .ExecuteDeleteAsync();

    await db.Notifications
        .Where(n => userIds.Contains(n.UserId) || n.Title.StartsWith(titlePrefix))
        .ExecuteDeleteAsync();

    await db.Reviews
        .Where(r => userIds.Contains(r.UserId) || courseIds.Contains(r.CourseId))
        .ExecuteDeleteAsync();

    await db.Quizzes
        .IgnoreQueryFilters()
        .Where(q => q.Title.StartsWith(titlePrefix) || (q.CourseId.HasValue && courseIds.Contains(q.CourseId.Value)))
        .ExecuteDeleteAsync();

    await db.TrainingClasses
        .IgnoreQueryFilters()
        .Where(c => c.Title.StartsWith(titlePrefix) || courseIds.Contains(c.CourseId))
        .ExecuteDeleteAsync();

    await db.Organizations
        .Where(o => o.Slug.StartsWith(slugPrefix))
        .ExecuteDeleteAsync();

    await db.Courses
        .IgnoreQueryFilters()
        .Where(c => courseIds.Contains(c.Id))
        .ExecuteDeleteAsync();

    await db.Users
        .Where(u => userIds.Contains(u.Id))
        .ExecuteDeleteAsync();
}

internal sealed record SeederOptions(
    string Profile,
    string Prefix,
    string ConnectionString,
    int BatchSize,
    string Password,
    bool ResetPrefix,
    bool Append)
{
    public static SeederOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
                continue;

            var key = arg[2..];
            if (key.Contains('=', StringComparison.Ordinal))
            {
                var parts = key.Split('=', 2);
                values[parts[0]] = parts[1];
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                values[key] = args[++i];
            else
                flags.Add(key);
        }

        var connectionString =
            Value(values, "connection-string") ??
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection") ??
            "Host=localhost;Port=5432;Database=elearning_dev;Username=postgres;Password=postgres";

        return new SeederOptions(
            Value(values, "profile") ?? "small",
            NormalizePrefix(Value(values, "prefix") ?? "seed"),
            connectionString,
            int.TryParse(Value(values, "batch-size"), out var batchSize) ? Math.Max(50, batchSize) : 1000,
            Value(values, "password") ?? "SeedPass123!",
            flags.Contains("reset-prefix"),
            flags.Contains("append"));
    }

    private static string? Value(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value : null;

    private static string NormalizePrefix(string value)
    {
        var normalized = new string(value.Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray()).Trim('-');

        return string.IsNullOrWhiteSpace(normalized) ? "seed" : normalized;
    }
}

internal sealed record SeedProfile(
    string Name,
    int UserCount,
    int OrganizationCount,
    int MembersPerOrganization,
    int CourseCount,
    int SectionsPerCourse,
    int LessonsPerSection,
    int TrainingClassCount,
    int SessionsPerClass,
    int QuizCount,
    int QuestionsPerQuiz,
    int ReviewCount,
    int ReviewsPerCourse,
    int NotificationCount,
    int VideoAssetCount,
    int WatchEventCount)
{
    public static SeedProfile Get(string name) =>
        name.ToLowerInvariant() switch
        {
            "small" => new("small", 50, 3, 12, 12, 3, 4, 18, 3, 12, 5, 60, 5, 150, 24, 300),
            "medium" => new("medium", 500, 12, 35, 80, 4, 5, 140, 4, 80, 8, 800, 10, 3_000, 250, 8_000),
            "large" => new("large", 4_000, 80, 80, 500, 5, 6, 1_000, 5, 500, 10, 5_000, 10, 30_000, 2_000, 80_000),
            _ => throw new ArgumentException($"Unknown profile '{name}'. Use small, medium, or large.")
        };
}

internal static partial class SeedText
{
    public static readonly string[] FirstNames =
    [
        "An", "Binh", "Chi", "Dung", "Giang", "Ha", "Khanh", "Lan", "Minh", "Nam",
        "Phuong", "Quang", "Thao", "Trang", "Tuan", "Vy"
    ];

    public static readonly string[] LastNames =
    [
        "Nguyen", "Tran", "Le", "Pham", "Hoang", "Huynh", "Vo", "Dang", "Bui", "Do"
    ];

    public static readonly string[] CourseTopics =
    [
        "AI", "Data Analytics", "Cloud Engineering", "Secure Coding", "Product Management",
        "UX Research", "Sales Enablement", "Leadership", "DevOps", "Backend Architecture"
    ];

    public static readonly string[] ModuleTitles =
    [
        "Foundations", "Applied Practice", "Team Workflow", "Quality Review", "Deployment",
        "Measurement", "Governance", "Capstone"
    ];

    public static readonly string[] LessonTitles =
    [
        "Core Concepts", "Hands-on Lab", "Case Study", "Common Pitfalls", "Checklist",
        "Assessment Prep", "Implementation Notes", "Operational Playbook"
    ];
}
