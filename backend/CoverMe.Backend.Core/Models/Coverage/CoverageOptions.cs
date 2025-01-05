using System.Text.RegularExpressions;

namespace CoverMe.Backend.Core.Models.Coverage;

public class CoverageOptions
{
    public bool Rebuild { get; init; }
    public bool HideAutoProperties { get; init; } = true;
    public string Filter { get; init; } = string.Empty;
    public string ParserExcluded { get; init; } = ".*\\.Tests$|testhost";
    public Regex ParserExcludedRegex => new(ParserExcluded);
}