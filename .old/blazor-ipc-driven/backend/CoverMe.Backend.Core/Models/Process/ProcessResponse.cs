namespace CoverMe.Backend.Core.Models.Process;

public class ProcessResponse
{
    public required int ExitCode { get; init; }
    public required string Output { get; init; }
    public required string Error { get; init; }
}