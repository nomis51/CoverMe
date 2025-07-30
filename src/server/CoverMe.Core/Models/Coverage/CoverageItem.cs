using CoverMe.Core.Enums;

namespace CoverMe.Core.Models.Coverage;

public class CoverageItem
{
    public required string Symbol { get; init; }
    public string? FilePath { get; set; }
    public int Coverage { get; set; }
    public required CoverageItemType Type { get; init; }
    public required CoverageItemVisibility Visibility { get; init; }
}