namespace CoverMe.Backend.Core.Models.Coverage;

public class CoverageOptions
{
    public bool Rebuild { get; init; }
    public string Filter { get; init; } = string.Empty;
}