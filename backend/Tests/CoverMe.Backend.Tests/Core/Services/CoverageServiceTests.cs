using System.IO.Abstractions;
using System.Reflection;
using CoverMe.Backend.Core.Enums.Coverage;
using CoverMe.Backend.Core.Enums.Process;
using CoverMe.Backend.Core.Helpers.Abstractions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;
using CoverMe.Backend.Core.Models.Process;
using CoverMe.Backend.Core.Services;
using CoverMe.Backend.Core.Services.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoverMe.Backend.Tests.Core.Services;

public class CoverageServiceTests
{
    #region Members

    private readonly IProcessHelper _processHelper;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<CoverageService> _logger;
    private readonly ICacheManager<Dictionary<int, bool?>> _linesCoverageCache;
    private readonly ICacheManager<List<CoverageNode>> _coverageTreeCache;
    private readonly ICoverageService _sut;

    #endregion

    #region Cosntructors

    public CoverageServiceTests()
    {
        _processHelper = Substitute.For<IProcessHelper>();
        _fileSystem = new FileSystem();
        _logger = Substitute.For<ILogger<CoverageService>>();
        _linesCoverageCache = Substitute.For<ICacheManager<Dictionary<int, bool?>>>();
        _coverageTreeCache = Substitute.For<ICacheManager<List<CoverageNode>>>();
        _sut = new CoverageService(_logger, _fileSystem, _linesCoverageCache!, _coverageTreeCache, _processHelper);
    }

    #endregion

    #region Tests

    [Fact]
    public void GetTestProjects_ShouldReturnProjects_WhenSessionExists()
    {
        // Arrange
        var expectedProjects = new List<Project>
        {
            Constants.SamplesTestAppTestsProject,
            Constants.SamplesTestAppOtherTestsProject
        };

        // Act
        var result = _sut.GetTestsProjects(Constants.SamplesSolution);

        // Assert
        result.Should().BeEquivalentTo(expectedProjects);
    }

    [Fact]
    public void GetTestsProjects_ShouldLogExceptionAndReturnEmptyList_WhenPathIsInvalid()
    {
        // Arrange
        const string path = "C:/Users/Tests/Some/Solution/Folder/";
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem
            .Directory.EnumerateFiles(path, "*.sln")
            .Returns([
                $"{path}SomeSolution.sln"
            ]);

        // Act
        var result = _sut.GetTestsProjects(new Solution(fileSystem, path));

        // Assert
        result.Should().BeEmpty();

        _logger.Received()
            .LogError(Arg.Any<Exception>(), "Failed to get tests projects");
    }

    [Fact]
    public void GetTestsProjects_ShouldReturnEmptyList_WhenSearchPatternMatchNothing()
    {
        // Arrange

        // Act
        var result = _sut.GetTestsProjects(Constants.SamplesSolution, "*.Oops.csproj");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTestsProjects_ShouldReturnOnlyMatchingTestsProjects()
    {
        // Arrange
        const int expectedCount = 1;
        const string searchPattern = "*.Other.Tests.csproj";
        var expectedProjects = new[]
        {
            Constants.SamplesTestAppOtherTestsProject,
        };

        // Act
        var result = _sut.GetTestsProjects(Constants.SamplesSolution, searchPattern);

        // Assert
        result.Should().HaveCount(expectedCount);
        result.Should().BeEquivalentTo(expectedProjects);
    }

    [Fact]
    public async Task ParseCoverage_ShouldShouldReturnNodes()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.Should().NotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;
        result.Should().HaveCount(11);
    }

    [Fact]
    public async Task ParseCoverage_ShouldHaveARootNodeRepresentingTheSolution()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.Should().NotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        result[0].Symbol.Should().Be("Solution");
        result[0].Level.Should().Be(0);
        result[0].FilePath.Should().Be(string.Empty);
        result[0].Icon.Should().Be(CoverageNodeIcon.Solution);
        result[0].Coverage.Should().Be(5);
        result[0].CoveredStatements.Should().Be(15);
        result[0].TotalStatements.Should().Be(331);
        result[0].IsExpanded.Should().Be(false);
        result[0].LineNumber.Should().Be(0);
    }

    [Fact]
    public async Task ParseCoverage_ShouldHaveFilePath_WhenNodesAreTypesOrMethods()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.Should().NotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        foreach (var node in result)
        {
            if (node.Type is not CoverageNodeType.MethodPropertyField and not CoverageNodeType.Type) continue;

            node.FilePath.Should().NotBeNullOrEmpty();

            try
            {
                _ = new FileInfo(node.FilePath);
            }
            catch (Exception)
            {
                Assert.Fail("Invalid file path: " + node.FilePath);
            }
        }
    }

    [Fact]
    public async Task ParseCoverage_ShouldHaveLineNumber_WhenNodesAreTypesOrMethods()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.Should().NotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        foreach (var node in result)
        {
            if (node.Type is not CoverageNodeType.MethodPropertyField and not CoverageNodeType.Type) continue;

            if (node.Type is CoverageNodeType.Type)
            {
                node.LineNumber.Should().Be(1);
            }
            else
            {
                node.LineNumber.Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public async Task ParseCoverage_ShouldReturnsAreTreeOfNodes()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.Should().NotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        for (var i = 0; i < result.Count; ++i)
        {
            var previousNode = i > 0 ? result[i - 1] : null;
            var node = result[i];

            if (node.Icon is CoverageNodeIcon.Solution || previousNode is null)
            {
                node.Level.Should().Be(0);
                continue;
            }

            if (previousNode.Icon < node.Icon)
            {
                node.Level.Should().Be(previousNode.Level + 1);
            }
            else if (previousNode.Icon > node.Icon)
            {
                node.Level.Should().Be(previousNode.Level - ((int)previousNode.Icon - 1));
            }
            else
            {
                node.Level.Should().Be(previousNode.Level);
            }
        }
    }

    [Fact]
    public async Task RunCoverage_ShouldRunCoverageAndReturnsNodes_WithDefaultOptions()
    {
        // Arrange
        var workingDirectory = Constants.SamplesTestAppTestsProject.FolderPath;
        _processHelper
            .DotCoverCli(Arg.Any<DotCoverCliOptions>())
            .Returns(new ProcessResponse
            {
                ExitCode = 0,
                Output = string.Empty,
                Error = string.Empty
            });
        var solution = Constants.SamplesSolution;

        // Act
        var result = await _sut.RunCoverage(
            solution,
            Constants.SamplesTestAppTestsProject,
            new CoverageOptions()
        );

        // Assert
        result.Should().BeNull();

        await _processHelper
            .Received()
            .DotCoverCli(
                Arg.Is<DotCoverCliOptions>(e =>
                    e.NoBuild &&
                    e.HideAutoProperties &&
                    e.Command == DotCoverCommand.CoverDotnet &&
                    e.ReportType == DotCoverReportType.DetailedXml &&
                    e.ProjectFolderPath == workingDirectory
                )
            );
    }

    #endregion

    #region Private methods

    private MethodInfo GetMethod_ParseCoverage()
    {
        return _sut.GetType()
            .GetMethod("ParseCoverage", BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    #endregion
}