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

        // Act
        var sut = new CoverageOptions();

        // Assert
        sut.Rebuild.Should().BeFalse();
        sut.Filter.Should().BeEmpty();
    }

    #endregion
}