using CoverMe.Backend.Core.Extensions;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Extensions;

public class StringExtensions
{
    #region Tests

    [InlineData("path/to/file.cs", "path/to/file.cs")]
    [InlineData(@"path\to\file.cs", "path/to/file.cs")]
    [InlineData(@"C:\path\to\file.cs", "C:/path/to/file.cs")]
    [InlineData("C:/path/to/file.cs", "C:/path/to/file.cs")]
    [Theory]
    public void ConvertPathToUnix_ShouldConvertPathToUnixFormat(string path, string expectedPath)
    {
        // Arrange

        // Act
        var result = path.ConvertPathToUnix();

        // Assert
        result.ShouldBe(expectedPath);
    }

    [InlineData("path/to/file.cs", @"path\to\file.cs")]
    [InlineData(@"path\to\file.cs", @"path\to\file.cs")]
    [InlineData(@"C:\path\to\file.cs", @"C:\path\to\file.cs")]
    [InlineData("C:/path/to/file.cs", @"C:\path\to\file.cs")]
    [Theory]
    public void ConvertPathToWindows_ShouldConvertPathToWindowsFormat(string path, string expectedPath)
    {
        // Arrange

        // Act
        var result = path.ConvertPathToWindows();

        // Assert
        result.ShouldBe(expectedPath);
    }

    #endregion
}