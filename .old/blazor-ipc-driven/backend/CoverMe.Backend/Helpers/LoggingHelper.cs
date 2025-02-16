using Serilog;

namespace CoverMe.Backend.Helpers;

public static class LoggingHelper
{
    #region Public methods

    public static void ConfigureLogging()
    {
        var config = new LoggerConfiguration()
            .WriteTo.Console();

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env != "Headless" && env != "Intellij")
        {
            config = config.WriteTo.File(
                Path.Join(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    $".{nameof(CoverMe)}",
                    "logs",
                    "backend-.txt"
                ),
                rollingInterval: RollingInterval.Day
            );
        }

        Log.Logger = config.CreateLogger();
    }

    #endregion
}