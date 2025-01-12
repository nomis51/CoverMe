namespace CoverMe.Backend.Models.JavaScript.Intellij;

public class ProjectSettings
{
    public required string ProjectRootPath { get; init; }
    public required string ChannelId { get; init; }
    public required IntellijTheme Theme { get; init; }
}