namespace CoverMe.Backend.Core.Models.Settings;

public class Settings
{
    public BackendSettings Backend { get; set; } = new();
    public CoverageSettings Coverage { get; set; } = new();
    public UiSettings Ui { get; set; } = new();
}