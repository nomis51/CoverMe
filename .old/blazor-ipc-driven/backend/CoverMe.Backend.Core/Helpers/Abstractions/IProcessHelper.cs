using CoverMe.Backend.Core.Models.Process;

namespace CoverMe.Backend.Core.Helpers.Abstractions;

public interface IProcessHelper
{
    Task<ProcessResponse> Execute(
        string command,
        IEnumerable<string> arguments,
        string workingDirectory
    );

    Task<ProcessResponse> DotCoverCli(DotCoverCliOptions options);
    Task<bool> EnsureDotCoverCliInstalled();
    Task<ProcessResponse> ReportGenerator(ReportGeneratorOptions options);
    Task<bool> EnsureReportGeneratorInstalled();
}