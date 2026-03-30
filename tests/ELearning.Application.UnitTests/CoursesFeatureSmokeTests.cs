using ELearning.Application.Features.Courses.CreateCourse;
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
}

