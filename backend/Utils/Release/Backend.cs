using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using HtmlAgilityPack;

namespace Release;

public class Backend
{
    #region Public methods

    public static async Task CalculateChecksum()
    {
        if (!File.Exists(Configuration.Backend.ReleaseZipFilePath))
        {
            Console.WriteLine("Backend zip file found: " + Configuration.Backend.ReleaseZipFilePath);
            Environment.Exit(1);
        }

        using var sha256 = SHA256.Create();
        await using var fs = File.OpenRead(Configuration.Backend.ReleaseZipFilePath);
        var bytes = await sha256.ComputeHashAsync(fs);
        var checksum = BitConverter.ToString(bytes)
            .Replace("-", "")
            .ToLowerInvariant();

        await File.WriteAllTextAsync(Configuration.Backend.ReleaseChecksumFilePath, checksum);
    }

    public static async Task Zip()
    {
        if (Directory.Exists(Configuration.OutputFolder))
        {
            Directory.Delete(Configuration.OutputFolder, true);
        }

        Directory.CreateDirectory(Configuration.OutputFolder);

        await using var fs = new FileStream(Configuration.Backend.ReleaseZipFilePath, FileMode.Create);
        using var archive = new ZipArchive(fs, ZipArchiveMode.Create);
        AddDirectoryToZip(archive, string.Empty, Configuration.Backend.ReleaseFolder);
    }

    public static async Task RunTests()
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = "dotnet",
                Arguments = string.Join(
                    " ",
                    "test",
                    "."
                ),
                WorkingDirectory = Configuration.Backend.TestsFolder,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("Backend tests failed:\n" + output + "\n" + error);
            Environment.Exit(process.ExitCode);
        }
    }

    public static async Task UpdateVersion(string projectFilePath)
    {
        if (!File.Exists(projectFilePath))
        {
            Console.WriteLine("Project file not found: " + projectFilePath);
            Environment.Exit(1);
        }

        var projectFileText = await File.ReadAllTextAsync(projectFilePath);
        if (string.IsNullOrEmpty(projectFileText))
        {
            Console.WriteLine("Project file is empty: " + projectFilePath);
            Environment.Exit(1);
        }

        var xml = new HtmlDocument();
        xml.LoadHtml(projectFileText);

        var propertyGroups = xml.DocumentNode.SelectNodes("//project/propertygroup");
        if (propertyGroups is null)
        {
            Console.WriteLine("Property groups not found in project file: " + Configuration.Backend.ProjectFilePath);
            Environment.Exit(1);
        }

        foreach (var propertyGroup in propertyGroups)
        {
            if (propertyGroup is null) continue;

            foreach (var child in propertyGroup.ChildNodes)
            {
                if (child is null) continue;
                if (!child.Name.Equals("TargetFramework", StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var versionNode in propertyGroup.ChildNodes)
                {
                    if (!versionNode.Name.Equals("version", StringComparison.OrdinalIgnoreCase) &&
                        !versionNode.Name.Equals("fileversion", StringComparison.OrdinalIgnoreCase) &&
                        !versionNode.Name.Equals("assemblyversion", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    versionNode.InnerHtml = Configuration.Version;
                }

                await using StringWriter writer = new();
                GenerateCaseSensitiveXml(xml.DocumentNode, writer);

                await File.WriteAllTextAsync(projectFilePath, writer.ToString());
                return;
            }
        }
    }

    public static async Task Publish()
    {
        if (Directory.Exists(Configuration.Backend.ReleaseFolder))
        {
            Directory.Delete(Configuration.Backend.ReleaseFolder, true);
        }

        var process = new Process
        {
            StartInfo =
            {
                FileName = "dotnet",
                Arguments = string.Join(
                    " ",
                    "publish",
                    "--configuration",
                    "Release",
                    "--output",
                    Configuration.Backend.ReleaseFolder
                ),
                WorkingDirectory = Configuration.Backend.BuildFolder,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("Backend build failed:\n" + output + "\n" + error);
            Environment.Exit(process.ExitCode);
        }
    }

    #endregion

    #region Private mthods

    private static void AddDirectoryToZip(ZipArchive zipArchive, string entryPath, string sourceFolder)
    {
        foreach (var file in Directory.GetFiles(sourceFolder))
        {
            var fileName = Path.GetFileName(file);
            var extension = Path.GetExtension(file);

            if (extension.EndsWith("pdb")) continue;
            if (fileName.StartsWith("appsettings.", StringComparison.OrdinalIgnoreCase) &&
                !fileName.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase)) continue;

            var path = Path.Combine(entryPath, fileName);
            zipArchive.CreateEntryFromFile(file, path);
        }

        foreach (var directory in Directory.GetDirectories(sourceFolder))
        {
            var path = Path.Combine(entryPath, Path.GetFileName(directory));
            AddDirectoryToZip(zipArchive, path, directory);
        }
    }

    private static void GenerateCaseSensitiveXml(HtmlNode node, StringWriter writer)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            writer.Write(node.InnerText);
            return;
        }

        if (node.Name != "#document")
        {
            writer.Write($"<{node.OriginalName}");

            foreach (var attribute in node.Attributes)
            {
                writer.Write($" {attribute.OriginalName}=\"{attribute.Value}\"");
            }

            if (node.ChildNodes.Count == 0 &&
                node.OuterHtml.EndsWith($"</{node.Name}>", StringComparison.OrdinalIgnoreCase))
            {
                writer.Write(" />");
                return;
            }

            writer.Write(">");
        }

        foreach (var childNode in node.ChildNodes)
        {
            GenerateCaseSensitiveXml(childNode, writer);
        }

        if (node.Name != "#document" && node.NodeType == HtmlNodeType.Element && !HtmlNode.IsEmptyElement(node.Name))
        {
            writer.Write($"</{node.OriginalName}>");
        }
    }

    #endregion
}