namespace CoverMe.Backend.Core.Models.Settings;

public class Settings
{
    public int BackendPort { get; set; } = 5263;
    public string ProjectFilter { get; set; } = "";
    public string TestsFilter { get; set; } = "";
}