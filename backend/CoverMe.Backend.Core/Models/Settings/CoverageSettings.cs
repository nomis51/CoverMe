namespace CoverMe.Backend.Core.Models.Settings;

public class CoverageSettings
{
    public string ProjectFilter { get; set; } = @".*\.Tests.csproj";
    public string TestsFilter { get; set; } = "FullyQualifiedName~.Tests";
    public string CoverageFilter { get; set; } = "-:module=testhost;-:module=*.Tests";
    public bool HideAutoProperties { get; set; } = true;
}