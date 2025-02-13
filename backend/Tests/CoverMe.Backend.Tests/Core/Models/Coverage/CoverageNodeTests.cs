using CoverMe.Backend.Core.Enums.Coverage;
using CoverMe.Backend.Core.Models.Coverage;
using Shouldly;

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
        sut.Level.ShouldBe(level);
        sut.Symbol.ShouldBe(symbol);
        sut.Icon.ShouldBe(icon);
        sut.Type.ShouldBe(type);
        sut.FilePath.ShouldBeEmpty();
    }

    #endregion
}