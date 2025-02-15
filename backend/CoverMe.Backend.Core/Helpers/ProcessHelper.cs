using System.Diagnostics;
using CoverMe.Backend.Core.Extensions;
using CoverMe.Backend.Core.Helpers.Abstractions;
using CoverMe.Backend.Core.Models.Process;
using Microsoft.Extensions.Logging;

namespace CoverMe.Backend.Core.Helpers;

public class ProcessHelper : IProcessHelper
{
    #region Members

    private readonly ILogger<ProcessHelper> _logger;
    private bool _isDotCoverCliInstalled;
    private bool _isReportGeneratorInstalled;
    private static readonly SemaphoreSlim DotCoverInstallationLock = new(1, 1);
    private static readonly SemaphoreSlim ReportGeneratorInstallationLock = new(1, 1);

    #endregion

    #region Constructors

    public ProcessHelper(ILogger<ProcessHelper> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Public methods

    public async Task<ProcessResponse> DotCoverCli(DotCoverCliOptions options)
    {
        if (!_isDotCoverCliInstalled)
        {
            if (!await EnsureDotCoverCliInstalled())
            {
                return new ProcessResponse
                {
                    ExitCode = 1,
                    Error = "Failed to install dotcover cli",
                    Output = string.Empty
                };
            }
        }

        var arguments = new List<string>
        {
            "dotcover",
            options.Command.Description(),
            $"--ReportType={options.ReportType.Description()}",
            $"--Output=\"{options.OutputPath}\"",
        };

        if (options.HideAutoProperties)
        {
            arguments.Add("--HideAutoProperties");
        }

        arguments.AddRange([
            "--",
            "test"
        ]);

        if (options.NoBuild)
        {
            arguments.Add("--no-build");
        }

        arguments.Add($"\"{options.ProjectFolderPath}\"");

        return await Execute(
            "dotnet",
            arguments,
            options.ProjectFolderPath
        );
    }

    public async Task<bool> EnsureDotCoverCliInstalled()
    {
        await DotCoverInstallationLock.WaitAsync();

        try
        {
            if (_isDotCoverCliInstalled) return true;

            var response = await Execute(
                "dotnet",
                [
                    "tool",
                    "install",
                    "--global",
                    "JetBrains.dotCover.CommandLineTools"
                ],
                "."
            );

            _isDotCoverCliInstalled = response.ExitCode == 0;

            if (!_isDotCoverCliInstalled)
            {
                _logger.LogWarning(
                    "Failed to install dotCover CLI: ExitCode={ExitCode}, Output={Output}, Error={Error}",
                    response.ExitCode, response.Output, response.Error);
            }
            else
            {
                _logger.LogTrace("DotCover CLI: {Output}", response.Output);
            }

            return _isDotCoverCliInstalled;
        }
        finally
        {
            DotCoverInstallationLock.Release();
        }
    }

    public async Task<ProcessResponse> ReportGenerator(ReportGeneratorOptions options)
    {
        if (!_isReportGeneratorInstalled)
        {
            if (!await EnsureReportGeneratorInstalled())
            {
                return new ProcessResponse
                {
                    ExitCode = 1,
                    Error = "Failed to install report generator",
                    Output = string.Empty
                };
            }
        }

        var arguments = new List<string>
        {
            $"-reports:\"{options.ReportFilePath}\"",
            $"-targetdir:\"{options.OutputFolderPath}\"",
        };

        return await Execute(
            "reportgenerator",
            arguments,
            Path.GetDirectoryName(options.ReportFilePath) ?? "."
        );
    }

    public async Task<bool> EnsureReportGeneratorInstalled()
    {
        await ReportGeneratorInstallationLock.WaitAsync();

        try
        {
            if (_isReportGeneratorInstalled) return true;

            var response = await Execute(
                "dotnet",
                [
                    "tool",
                    "install",
                    "--global",
                    "dotnet-reportgenerator-globaltool"
                ],
                "."
            );

            _isReportGeneratorInstalled = response.ExitCode == 0;

            if (!_isReportGeneratorInstalled)
            {
                _logger.LogWarning(
                    "Failed to install report generator: ExitCode={ExitCode}, Output={Output}, Error={Error}",
                    response.ExitCode, response.Output, response.Error);
            }
            else
            {
                _logger.LogTrace("Report Generator: {Output}", response.Output);
            }

            return _isReportGeneratorInstalled;
        }
        finally
        {
            ReportGeneratorInstallationLock.Release();
        }
    }

    public async Task<ProcessResponse> Execute(
        string command,
        IEnumerable<string> arguments,
        string workingDirectory
    )
    {
        var argumentsStr = string.Join(" ", arguments);

        _logger.LogInformation(
            "ProcessHelper: Executed: Command={Command}; Arguments={Arguments}; WorkingDirectory={WorkingDirectory};",
            command,
            argumentsStr,
            workingDirectory
        );
        var process = new Process
        {
            StartInfo =
            {
                FileName = command,
                Arguments = argumentsStr,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            },
        };

        if (!process.Start()) throw new ApplicationException();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        _logger.LogInformation(
            "ProcessHelper: Executed: Command={Command}; Arguments={Arguments}; WorkingDirectory={WorkingDirectory}; ExitCode={ExitCode}; Output={Output}; Error={Error};",
            command,
            argumentsStr,
            workingDirectory,
            process.ExitCode,
            output,
            error
        );

        return new ProcessResponse
        {
            ExitCode = process.ExitCode,
            Output = output,
            Error = error
        };
    }

    #endregion
}