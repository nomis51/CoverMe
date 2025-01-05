namespace CoverMe.Backend.Core.Models;

public class Project
{
    public string FilePath { get; set; }
    public string Name => Path.GetFileNameWithoutExtension(FilePath);
    public string FolderPath => Path.GetDirectoryName(FilePath) ?? string.Empty;

    public Project(string csprojFilePath)
    {
        FilePath = csprojFilePath;
    }
}