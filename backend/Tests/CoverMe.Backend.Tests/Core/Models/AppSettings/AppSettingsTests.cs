using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace CoverMe.Backend.Tests.Core.Models.AppSettings;

public class AppSettingsTests
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
        var sut = new Backend.Core.Models.AppSettings.AppSettings(configuration);

        // Assert
        sut.Intellij.ProjectRootPath.Should().Be("SomePath");
    }

    #endregion
}