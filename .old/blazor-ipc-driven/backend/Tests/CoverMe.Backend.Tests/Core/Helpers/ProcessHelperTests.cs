using System.Reflection;
using CoverMe.Backend.Core.Enums.Process;
using CoverMe.Backend.Core.Helpers;
using CoverMe.Backend.Core.Models.Process;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Helpers;

public class ProcessHelperTests
{
    #region Members

    private readonly ILogger<ProcessHelper> _logger = Substitute.For<ILogger<ProcessHelper>>();
    private readonly ProcessHelper _sut;

    #endregion

    #region Constructors

    public ProcessHelperTests()
    {
        _sut = new ProcessHelper(_logger);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task DotCoverCli_ShouldEnsureDotCoverCliIsInstalled_WhenNotInstalled()
    {
        // Arrange

        // Act
        _ = await _sut.DotCoverCli(new DotCoverCliOptions
        {
            Command = DotCoverCommand.CoverDotnet,
            CoverageFilter = string.Empty,
            HideAutoProperties = true,
            NoBuild = true,
            OutputPath = Path.GetTempPath(),
            ProjectFolderPath = Constants.SamplesTestAppTestsProject.FolderPath,
            ReportType = DotCoverReportType.DetailedXml,
            TestsFilter = string.Empty,
        });

        // Act
        _logger.Received()
            .Log(
                LogLevel.Trace,
                0,
                Arg.Is<object>(e => e.ToString()!
                    .StartsWith("DotCover CLI: ")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task DotCoverCli_ShouldNoCheckInstallation_WhenAlreadyInstalled()
    {
        // Arrange
        _sut.GetType()
            .GetField("_isDotCoverCliInstalled", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(_sut, true);

        // Act
        _ = await _sut.DotCoverCli(new DotCoverCliOptions
        {
            Command = DotCoverCommand.CoverDotnet,
            CoverageFilter = string.Empty,
            HideAutoProperties = true,
            NoBuild = true,
            OutputPath = Path.GetTempPath(),
            ProjectFolderPath = Constants.SamplesTestAppTestsProject.FolderPath,
            ReportType = DotCoverReportType.DetailedXml,
            TestsFilter = string.Empty,
        });

        // Act
        _logger.Received(0);
    }

    [Fact]
    public async Task DotCoverCli_ShouldGenerateDetailedXmlReport()
    {
        // Arrange
        var outputPath = Path.Join(Path.GetTempPath(), "report.xml");

        // Act
        _ = await _sut.DotCoverCli(new DotCoverCliOptions
        {
            Command = DotCoverCommand.CoverDotnet,
            CoverageFilter = string.Empty,
            HideAutoProperties = true,
            NoBuild = true,
            OutputPath = outputPath,
            ProjectFolderPath = Constants.SamplesTestAppTestsProject.FolderPath,
            ReportType = DotCoverReportType.DetailedXml,
            TestsFilter = string.Empty,
        });

        // Assert
        try
        {
            File.Exists(outputPath).ShouldBeTrue();
            (await File.ReadAllTextAsync(outputPath)).ShouldNotBeEmpty();
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task ReportGenerator_ShouldEnsureReportGeneratorIsInstalled_WhenNotInstalled()
    {
        // Arrange
        var outputPath = Path.Join(Path.GetTempPath(), "report");

        // Act
        _ = await _sut.ReportGenerator(new ReportGeneratorOptions
        {
            OutputFolderPath = outputPath,
            ReportFilePath = Path.Join(Constants.SamplesSolution.FolderPath, ".coverage", "report.xml"),
            ReportType = ReportGeneratorReportType.Html
        });

        // Act
        _logger.Received()
            .Log(
                LogLevel.Trace,
                0,
                Arg.Is<object>(e => e.ToString()!
                    .StartsWith("Report Generator: ")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task ReportGenerator_ShouldNoCheckInstallation_WhenAlreadyInstalled()
    {
        // Arrange
        _sut.GetType()
            .GetField("_isReportGeneratorInstalled", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(_sut, true);

        // Act
        _ = await _sut.ReportGenerator(new ReportGeneratorOptions
        {
            OutputFolderPath = Path.Join(Path.GetTempPath(), "report"),
            ReportFilePath = Path.Join(Constants.SamplesSolution.FolderPath, ".coverage", "report.xml"),
            ReportType = ReportGeneratorReportType.Html
        });

        // Act
        _logger.Received(0);
    }

    [Fact]
    public async Task ReportGenerator_ShouldGenerateHtmlReport()
    {
        // Arrange
        var outputPath = Path.Join(Path.GetTempPath(), "report");

        // Act
        _ = await _sut.ReportGenerator(new ReportGeneratorOptions
        {
            OutputFolderPath = outputPath,
            ReportFilePath = Path.Join(Constants.SamplesSolution.FolderPath, ".coverage", "report.xml"),
            ReportType = ReportGeneratorReportType.Html
        });

        // Assert
        try
        {
            Directory.Exists(outputPath).ShouldBeTrue();
            (await File.ReadAllTextAsync(Path.Join(outputPath, "index.html"))).ShouldNotBeEmpty();
        }
        finally
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    #endregion
}