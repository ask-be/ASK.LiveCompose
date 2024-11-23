using FluentAssertions;

namespace ASK.LiveCompose.Tests;

public class ProjectAndServiceNameTests
{
    [Theory]
    [InlineData("aaa")]
    [InlineData("aaa-bbb")]
    [InlineData("aaa_bbb")]
    [InlineData("aaa234-b3bb")]
    [InlineData("MixedCase")]
    [InlineData("EndsWithNumber3")]
    [InlineData("projectwith.dot")]
    public void ProjectNameValidTest(string name)
    {
        name.IsValidProjectName().Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("with spaces")]
    public void ProjectNameInvalidTest(string name)
    {
        name.IsValidProjectName().Should().BeFalse();
    }
}