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
    public void ProjectAndServiceNameValidTest(string name)
    {
        name.IsValidateServiceOrProjectName().Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("with spaces")]
    [InlineData("1startWithNumber")]
    [InlineData("-startWithDash")]
    [InlineData("_startWithUnderscore")]
    [InlineData("endWithDash-")]
    [InlineData("endWithUnderscore_")]
    public void ProjectAndServiceNameInvalidTest(string name)
    {
        name.IsValidateServiceOrProjectName().Should().BeFalse();
    }
}