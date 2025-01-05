using CoverMe.Backend.Core.Enums.Process;

namespace CoverMe.Backend.Core.Models.Process;

public class DotCoverCliOptions
{
    public required DotCoverCommand Command { get; init; }
    public required DotCoverReportType ReportType { get; init; }
    public required string OutputPath { get; init; }
    public required string ProjectFolderPath { get; init; }
    public bool HideAutoProperties { get; init; } = true;
    public bool NoBuild { get; init; } = true;
}