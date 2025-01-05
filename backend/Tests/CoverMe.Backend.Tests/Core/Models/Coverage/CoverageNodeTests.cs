using CoverMe.Backend.Core.Enums.Coverage;
using CoverMe.Backend.Core.Models.Coverage;
using FluentAssertions;

namespace CoverMe.Backend.Tests.Core.Models.Coverage;

public class CoverageNodeTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldCreateCoverageNode()
    {
        // Arrange
        const int level = 4;
        const string symbol = "potato";
        const CoverageNodeIcon icon = CoverageNodeIcon.Type;
        const CoverageNodeType type = CoverageNodeType.Solution;

        // Act
        var sut = new CoverageNode(level, symbol, icon, type);

        // Assert
        sut.Level.Should().Be(level);
        sut.Symbol.Should().Be(symbol);
        sut.Icon.Should().Be(icon);
        sut.Type.Should().Be(type);
        sut.FilePath.Should().BeEmpty();
    }

    #endregion
}