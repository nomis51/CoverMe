namespace CoverMe.Backend.Models.JavaScript.Intellij;

public class IntellijTheme
{
    public IntellijBaseTheme Panel { get; set; } = new();
    public IntellijBaseTheme Editor { get; set; } = new();
    public IntellijBaseTheme Button { get; set; } = new();
    public IntellijBaseTheme TextField { get; set; } = new();
    public IntellijColor Colors { get; set; } = new();
}

public class IntellijColor
{
    public string AccentColor { get; set; } = string.Empty;
}

public class IntellijBaseTheme
{
    public string Background { get; set; } = string.Empty;
    public string Foreground { get; set; } = string.Empty;
}