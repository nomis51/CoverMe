using System.IO.Abstractions;

namespace CoverMe.Backend.Core.Models;

public class Solution
{
    public string FilePath { get; }
    public string FolderPath { get; }
    public string Name => Path.GetFileNameWithoutExtension(FilePath);
    public string IdeaPluginFolder => Path.Join(FolderPath, ".idea", "coverme");

    public Solution(IFileSystem fileSystem, string folderPath)
    {
        FolderPath = folderPath;
        FilePath = fileSystem.Directory
            .EnumerateFiles(folderPath, "*.sln")
            .FirstOrDefault() ?? string.Empty;
    }
}