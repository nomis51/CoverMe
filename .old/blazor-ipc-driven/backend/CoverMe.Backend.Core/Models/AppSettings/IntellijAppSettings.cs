using Microsoft.Extensions.Configuration;

namespace CoverMe.Backend.Core.Models.AppSettings;

public class IntellijAppSettings
{
    private const string SectionName = "Intellij";
    public string ProjectRootPath { get; }

    public IntellijAppSettings(IConfiguration configuration)
    {
        ProjectRootPath = configuration[$"{SectionName}:ProjectRootPath"] ?? string.Empty;
    }
}