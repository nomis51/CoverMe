using System.Text.Json;
using CoverMe.Backend.Core.Models.Ipc;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Models.Ipc;

public class GetFileLineCoverageRequestTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldSerializeTheFieldsProperly()
    {
        // Arrange
        const string filePath = "path/to/file.cs";
        const int line = 42;
        const string projectRootPath = "path/to/project";
        var sut = new GetFileLineCoverageRequest
        {
            FilePath = filePath,
            Line = line,
            ProjectRootPath = projectRootPath
        };

        // Act
        var serialized = JsonSerializer.Serialize(sut);

        // Assert
        serialized.ShouldBe(
            $"{{\"filePath\":\"{filePath}\",\"line\":{line},\"projectRootPath\":\"{projectRootPath}\"}}");
    }

    #endregion
}