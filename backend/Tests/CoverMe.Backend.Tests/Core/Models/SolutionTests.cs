﻿using System.IO.Abstractions;
using CoverMe.Backend.Core.Models;
using FluentAssertions;
using NSubstitute;

namespace CoverMe.Backend.Tests.Core.Models;

public class SolutionTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldCreateSolution_WhenPathIsValid()
    {
        // Arrange
        const string path = @"C:\Users\Tests\Some\Solution\Folder\";
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem
            .Directory
            .EnumerateFiles(path, "*.sln")
            .Returns([
                $"{path}SomeSolution.sln"
            ]);
        // Act
        var solution = new Solution(fileSystem, path);

        // Assert
        solution.FilePath.Should().Be($"{path}SomeSolution.sln");
        solution.Name.Should().Be("SomeSolution");
        solution.FolderPath.Should().Be(path);
    }

    [Fact]
    public void Ctor_ShouldThrowException_WhenPathIsInvalid()
    {
        // Arrange
        const string path = @"C:\Users\Tests\Some\Solution\Folder\That\Does\Not\Exist\";
        var fileSystem = new FileSystem();

        // Act
        // Assert
        Assert.ThrowsAny<Exception>(() =>
            new Solution(fileSystem, path)
        );
    }

    #endregion
}