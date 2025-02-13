using CoverMe.Backend.Core.Models;
using Shouldly;

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
        project.FilePath.ShouldBe(csprojFilePath);
        project.Name.ShouldBe("Project");
        project.FolderPath.ShouldBe(path);
    }

    #endregion
}