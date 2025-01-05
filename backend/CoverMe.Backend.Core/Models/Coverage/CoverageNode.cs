using CoverMe.Backend.Core.Enums.Coverage;

namespace CoverMe.Backend.Core.Models.Coverage;

public class CoverageNode
{
    public string Symbol { get; }
    public int Coverage { get; set; }
    public int CoveredStatements { get; set; }
    public int TotalStatements { get; set; }
    public CoverageNodeIcon Icon { get; }
    public CoverageNodeType Type { get; }
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; init; }
    public int Level { get; }
    public bool IsExpanded { get; set; }

    public CoverageNode(int level, string symbol, CoverageNodeIcon icon, CoverageNodeType type)
    {
        Level = level;
        Symbol = symbol;
        Icon = icon;
        Type = type;
    }
}