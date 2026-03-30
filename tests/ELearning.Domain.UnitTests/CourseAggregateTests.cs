using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class CourseAggregateTests
{
    [Fact]
    public void Create_sets_status_draft()
    {
        var c = Course.Create("Intro to C#", "desc");
        c.Status.Should().Be(CourseStatus.Draft);
        c.Title.Should().Be("Intro to C#");
    }

    [Fact]
    public void Publish_without_lessons_throws()
    {
        var c = Course.Create("T", null);
        c.AddSection("S1");

        var act = () => c.Publish();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Publish_with_lesson_sets_status_published()
    {
        var c = Course.Create("T", null);
        var s = c.AddSection("S1");
        s.AddLesson("L1");

        c.Publish();
        c.Status.Should().Be(CourseStatus.Published);
    }
}

