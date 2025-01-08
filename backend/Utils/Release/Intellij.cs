using System.Diagnostics;

namespace Release;

public class Intellij
{
    #region Public methods

    public static void MoveJarToOutputFolder()
    {
        if (!File.Exists(Configuration.Intellij.JarFilePath))
        {
            Console.WriteLine("Intellij jar file not found: " + Configuration.Intellij.JarFilePath);
            Environment.Exit(1);
        }

        File.Move(
            Configuration.Intellij.JarFilePath,
            Configuration.Intellij.ReleaseFilePath
        );
    }

    public static async Task BuildPlugin()
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = Configuration.Intellij.GradlewFilePasth,
                Arguments = "buildPlugin",
                WorkingDirectory = Configuration.Intellij.PluginFolder,
                UseShellExecute = false,
                CreateNoWindow = false
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Environment.Exit(process.ExitCode);
        }
    }

    public static async Task UpdateVersion()
    {
        var lines = await File.ReadAllLinesAsync(Configuration.Intellij.GradlePropertiesFilePath);
        if (lines.Length == 0)
        {
            Console.WriteLine("Gradle properties file is empty: " + Configuration.Intellij.GradlePropertiesFilePath);
            Environment.Exit(1);
        }

        for (var i = 0; i < lines.Length; ++i)
        {
            if (!lines[i].StartsWith("PluginVersion=")) continue;

            lines[i] = $"PluginVersion={Configuration.Version}";

            await File.WriteAllLinesAsync(Configuration.Intellij.GradlePropertiesFilePath, lines);
            return;
        }

        Console.WriteLine("Plugin version not found in gradle properties file: " +
                          Configuration.Intellij.GradlePropertiesFilePath);
        Environment.Exit(1);
    }

    #endregion
}