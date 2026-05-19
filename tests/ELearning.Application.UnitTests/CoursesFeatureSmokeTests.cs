using ELearning.Application.Features.Courses.CreateCourse;
using ELearning.Application.Features.Courses.ListCourses;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class CoursesFeatureSmokeTests
{
    [Fact]
    public void CreateCourseValidator_rejects_empty_title()
    {
        var v = new CreateCourseCommandValidator();
        var result = v.Validate(new CreateCourseCommand("", null));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ListCoursesValidator_rejects_invalid_price_range()
    {
        var v = new ListCoursesQueryValidator();

        var result = v.Validate(new ListCoursesQuery(MinPriceCents: 10_000, MaxPriceCents: 5_000));

        result.IsValid.Should().BeFalse();
    }
}
