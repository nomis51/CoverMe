using CoverMe.Backend.Core.Enums.Process;

namespace CoverMe.Backend.Core.Models.Process;

public class ReportGeneratorOptions
{
    public required string ReportFilePath { get; init; }
    public required string OutputFolderPath { get; init; }
    public required ReportGeneratorReportType ReportType { get; init; } = ReportGeneratorReportType.Html;
}