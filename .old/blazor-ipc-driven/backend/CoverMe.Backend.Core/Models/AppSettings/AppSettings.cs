using Microsoft.Extensions.Configuration;

namespace CoverMe.Backend.Core.Models.AppSettings;

public class AppSettings
{
    public IntellijAppSettings Intellij { get; }

    public AppSettings(IConfiguration configuration)
    {
        Intellij = new IntellijAppSettings(configuration);
    }
}