using ELearning.Application.Common;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Foo   Bar  ", "foo-bar")]
    public void Slugify_normalizes(string input, string expected) =>
        SlugHelper.Slugify(input).Should().Be(expected);
}
