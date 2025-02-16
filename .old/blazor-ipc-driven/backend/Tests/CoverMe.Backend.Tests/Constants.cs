using System.IO.Abstractions;
using System.Reflection;
using CoverMe.Backend.Core.Models;

namespace CoverMe.Backend.Tests;

public static class Constants
{
    private static readonly string SamplesSolutionFolderPath = Path.GetFullPath(
        Path.Join(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "..",
            "..",
            "..",
            "..",
            "samples"
        )
    );

    public static readonly Solution SamplesSolution = new(new FileSystem(), SamplesSolutionFolderPath);

    public static Project SamplesTestAppTestsProject { get; } = new(
        Path.Join(
            SamplesSolution.FolderPath,
            "TestApp.Tests",
            "TestApp.Tests.csproj"
        )
    );

    public static Project SamplesTestAppOtherTestsProject { get; } = new(
        Path.Join(
            SamplesSolution.FolderPath,
            "Tests",
            "TestApp.Other.Tests",
            "TestApp.Other.Tests.csproj"
        )
    );

    public static Project SamplesTestAppNopeTestsProject { get; } = new(
        Path.Join(
            SamplesSolution.FolderPath,
            "Tests",
            "TestApp.NopeTests",
            "TestApp.NopeTests.csproj"
        )
    );

    public static string SamplesReportFilePath { get; } = Path.Join(
        SamplesSolution.FolderPath,
        ".coverage",
        "report.xml"
    );
}