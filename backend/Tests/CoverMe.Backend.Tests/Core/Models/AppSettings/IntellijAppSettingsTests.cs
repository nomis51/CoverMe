using CoverMe.Backend.Core.Models.AppSettings;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Models.AppSettings;

public class IntellijAppSettingsTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldCreate()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Intellij:ProjectRootPath"] = "SomePath"
            }!)
            .Build();

        // Act
        var sut = new IntellijAppSettings(configuration);

        // Assert
        sut.ProjectRootPath.ShouldBe("SomePath");
    }

    #endregion
}