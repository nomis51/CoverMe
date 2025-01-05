using System.Text.RegularExpressions;
using CoverMe.Backend.Core.Models.Coverage;
using FluentAssertions;

namespace CoverMe.Backend.Tests.Core.Models.Coverage;

public class CoverageOptionsTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldCreate_WithDefaultValues()
    {
        // Arrange
        const string parserExcluded = ".*\\.Tests$|testhost";
        var parserExcludedRegex = new Regex(parserExcluded);

        // Act
        var sut = new CoverageOptions();

        // Assert
        sut.Rebuild.Should().BeFalse();
        sut.HideAutoProperties.Should().BeTrue();
        sut.Filter.Should().BeEmpty();
        sut.ParserExcluded.Should().Be(parserExcluded);
        sut.ParserExcludedRegex.Should().BeEquivalentTo(parserExcludedRegex);
    }

    #endregion
}