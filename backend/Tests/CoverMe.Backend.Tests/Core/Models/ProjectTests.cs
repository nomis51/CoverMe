using CoverMe.Backend.Core.Models;
using FluentAssertions;

namespace CoverMe.Backend.Tests.Core.Models;

public class ProjectTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldCreateProject_WhenPathIsValid()
    {
        // Arrange
        const string path = @"C:\Users\Tests\Some\Project\Folder";
        const string csprojFilePath = $@"{path}\Project.csproj";

        // Act
        var project = new Project(csprojFilePath);

        // Assert
        project.FilePath.Should().Be(csprojFilePath);
        project.Name.Should().Be("Project");
        project.FolderPath.Should().Be(path);
    }

    #endregion
}