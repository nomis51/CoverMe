using Release;
using Spectre.Console;

Configuration.Version = "1.0.0";

if (Directory.Exists(Configuration.OutputFolder))
{
    Directory.Delete(Configuration.OutputFolder, true);
}

Directory.CreateDirectory(Configuration.OutputFolder);

await AnsiConsole.Status()
    .StartAsync("Running backend tests", async _ => await Backend.RunTests());
await AnsiConsole.Status()
    .StartAsync("Update backend version", async _ =>
    {
        await Backend.UpdateVersion(Configuration.Backend.ProjectFilePath);
        await Backend.UpdateVersion(Configuration.Backend.ProjectCoreFilePath);
    });
await AnsiConsole.Status()
    .StartAsync("Publishing backend", async _ => await Backend.Publish());
await AnsiConsole.Status()
    .StartAsync("Zipping backend", async _ => await Backend.Zip());
await AnsiConsole.Status()
    .StartAsync("Calculating checksum", async _ => await Backend.CalculateChecksum());

await AnsiConsole.Status()
    .StartAsync("Update kotlin plugin infos", async _ => await Intellij.UpdateInfos());
await AnsiConsole.Status()
    .StartAsync("Building kotlin plugin", async _ => await Intellij.BuildPlugin());
AnsiConsole.Status()
    .Start("Moving kotlin plugin", _ => Intellij.MoveJarToOutputFolder());
await AnsiConsole.Status()
    .StartAsync("Cleaning kotlin plugin", async _ => await Intellij.Cleanup());