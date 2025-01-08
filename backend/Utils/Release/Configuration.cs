namespace Release;

public class Configuration
{
    public static string Version = "0.1.0";

    private static readonly string RootFolder = Path.GetFullPath(Path.Join("..", "..", "..", "..", "..", ".."));

    public static readonly string OutputFolder = Path.GetFullPath(
        Path.Join(
            RootFolder,
            "build"
        )
    );

    public class Intellij
    {
        public static readonly string PluginFolder = Path.Join(
            RootFolder,
            "plugin"
        );

        public static readonly string GradlewFilePasth = Path.Join(
            PluginFolder,
            "gradlew.bat"
        );

        public static readonly string GradlePropertiesFilePath = Path.Join(
            PluginFolder,
            "gradle.properties"
        );

        public static readonly string JarFilePath = Path.Join(
            PluginFolder,
            "build",
            "libs",
            $"ReSharperPlugin.CoverMe-{Version}.jar"
        );

        public static readonly string ReleaseFilePath = Path.Join(
            OutputFolder,
            $"ReSharperPlugin.CoverMe-{Version}.jar"
        );
    }

    public class Backend
    {
        private const string DotnetVersion = "net8.0";
        private const string DotnetConfiguration = "Release";

        public static readonly string TestsFolder = Path.Join(
            RootFolder,
            "backend",
            "Tests",
            "CoverMe.Backend.Tests"
        );

        public static readonly string BuildFolder = Path.Join(
            RootFolder,
            "backend",
            "CoverMe.Backend"
        );

        public static readonly string ProjectFilePath = Path.Join(
            RootFolder,
            "backend",
            "CoverMe.Backend",
            "CoverMe.Backend.csproj"
        );

        public static readonly string ProjectCoreFilePath = Path.Join(
            RootFolder,
            "backend",
            "CoverMe.Backend.Core",
            "CoverMe.Backend.Core.csproj"
        );

        public static readonly string ReleaseFolder = Path.Join(
            RootFolder,
            "backend",
            "CoverMe.Backend",
            "bin",
            DotnetConfiguration,
            DotnetVersion
        );

        public static readonly string ReleaseZipFilePath = Path.Join(
            OutputFolder,
            "CoverMe.Backend.zip"
        );

        public static readonly string ReleaseChecksumFilePath = Path.Join(
            OutputFolder,
            "CoverMe.Backend.checksum"
        );
    }
}