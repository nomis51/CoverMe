﻿using System.IO.Abstractions;
using System.Reflection;
using CoverMe.Backend.Core.Enums.Coverage;
using CoverMe.Backend.Core.Enums.Process;
using CoverMe.Backend.Core.Helpers.Abstractions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;
using CoverMe.Backend.Core.Models.Process;
using CoverMe.Backend.Core.Models.Settings;
using CoverMe.Backend.Core.Services;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Services;

public class CoverageServiceTests
{
    #region Members

    private readonly IProcessHelper _processHelper;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<CoverageService> _logger;
    private readonly ICacheManager<Dictionary<int, bool?>> _linesCoverageCache;
    private readonly ICacheManager<List<CoverageNode>> _coverageTreeCache;
    private readonly ISettingsService _settingsService;
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
        _settingsService = Substitute.For<ISettingsService>();
        _settingsService.GetSettingsAsync()
            .Returns(new Settings());
        _sut = new CoverageService(_logger, _fileSystem, _linesCoverageCache!, _coverageTreeCache, _processHelper,
            _settingsService);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task GetTestProjects_ShouldReturnProjects_WhenSessionExists()
    {
        // Arrange
        var expectedProjects = new List<Project>
        {
            Constants.SamplesTestAppTestsProject,
            Constants.SamplesTestAppOtherTestsProject
        };

        // Act
        var result = await _sut.GetTestsProjects(Constants.SamplesSolution);

        // Assert
        result.ShouldBeEquivalentTo(expectedProjects);
    }

    [Fact]
    public async Task GetTestsProjects_ShouldLogExceptionAndReturnEmptyList_WhenPathIsInvalid()
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
        var result = await _sut.GetTestsProjects(new Solution(fileSystem, path));

        // Assert
        result.ShouldBeEmpty();

        _logger.Received()
            .LogError(Arg.Any<Exception>(), "Failed to get tests projects");
    }

    [Fact]
    public async Task GetTestsProjects_ShouldReturnEmptyList_WhenSearchPatternMatchNothing()
    {
        // Arrange
        _settingsService.GetSettingsAsync()
            .Returns(new Settings
            {
                Coverage =
                {
                    ProjectsFilter = "*.Invalid.Tests.csproj"
                }
            });

        // Act
        var result = await _sut.GetTestsProjects(Constants.SamplesSolution);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetTestsProjects_ShouldReturnOnlyMatchingTestsProjects()
    {
        // Arrange
        const int expectedCount = 1;
        const string searchPattern = "*.Other.Tests.csproj";
        var expectedProjects = new[]
        {
            Constants.SamplesTestAppOtherTestsProject,
        }.ToList();
        _settingsService.GetSettingsAsync()
            .Returns(new Settings
            {
                Coverage =
                {
                    ProjectsFilter = searchPattern
                }
            });

        // Act
        var result = await _sut.GetTestsProjects(Constants.SamplesSolution);

        // Assert
        result.Count.ShouldBe(expectedCount);
        result.ShouldBeEquivalentTo(expectedProjects);
    }

    [Fact]
    public async Task ParseCoverage_ShouldReturnNodes()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.ShouldNotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;
        result.Count.ShouldBe(16);
    }

    [Fact]
    public async Task ParseCoverage_ShouldHaveARootNodeRepresentingTheSolution()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.ShouldNotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        result[0].Symbol.ShouldBe("Solution");
        result[0].Level.ShouldBe(0);
        result[0].FilePath.ShouldBe(string.Empty);
        result[0].Icon.ShouldBe(CoverageNodeIcon.Solution);
        result[0].Coverage.ShouldBe(64);
        result[0].CoveredStatements.ShouldBe(23);
        result[0].TotalStatements.ShouldBe(36);
        result[0].IsExpanded.ShouldBe(false);
        result[0].LineNumber.ShouldBe(0);
    }

    [Fact]
    public async Task ParseCoverage_ShouldHaveFilePath_WhenNodesAreTypesOrMethods()
    {
        // Arrange
        var sut = GetMethod_ParseCoverage();

        // Act
        var task = sut.Invoke(_sut, [Constants.SamplesReportFilePath, new CoverageOptions()]);

        // Assert
        task.ShouldNotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        foreach (var node in result)
        {
            if (node.Type is not CoverageNodeType.MethodPropertyField and not CoverageNodeType.Type) continue;

            node.FilePath.ShouldNotBeNullOrEmpty();

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
        task.ShouldNotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        foreach (var node in result)
        {
            if (node.Type is not CoverageNodeType.MethodPropertyField and not CoverageNodeType.Type) continue;

            if (node.Type is CoverageNodeType.Type)
            {
                node.LineNumber.ShouldBe(1);
            }
            else
            {
                node.LineNumber.ShouldBeGreaterThan(0);
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
        task.ShouldNotBeNull();
        var result = await (Task<List<CoverageNode>>)task!;

        for (var i = 0; i < result.Count; ++i)
        {
            var previousNode = i > 0 ? result[i - 1] : null;
            var node = result[i];

            if (node.Icon is CoverageNodeIcon.Solution || previousNode is null)
            {
                node.Level.ShouldBe(0);
                continue;
            }

            if (previousNode.Icon < node.Icon)
            {
                node.Level.ShouldBe(previousNode.Level + 1);
            }
            else if (previousNode.Icon > node.Icon)
            {
                node.Level.ShouldBe(previousNode.Level - ((int)previousNode.Icon - 1));
            }
            else
            {
                node.Level.ShouldBe(previousNode.Level);
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
        result.ShouldBeNull();

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