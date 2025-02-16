using System.IO.Abstractions;
using System.Text.Json;
using CoverMe.Backend.Core.Models.Settings;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Backend.Core.Services;

public class SettingsService : ISettingsService
{
    #region Constants

    private const string SettingsFileName = "settings.json";

    private static readonly string SettingsFilePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        $".{nameof(CoverMe).ToLower()}",
        SettingsFileName
    );

    #endregion

    #region Members

    private readonly ILogger<SettingsService> _logger;
    private readonly IFileSystem _fileSystem;

    #endregion

    #region Constructors

    public SettingsService(ILogger<SettingsService> logger, IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    #endregion

    #region Public methods

    public async Task<Settings> GetSettingsAsync()
    {
        try
        {
            if (!_fileSystem.File.Exists(SettingsFilePath)) return new Settings();

            var data = await _fileSystem.File.ReadAllTextAsync(SettingsFilePath);
            return JsonSerializer.Deserialize<Settings>(data) ?? new Settings();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get settings");
        }

        return new Settings();
    }

    public async Task UpdateSettingsAsync(Settings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings);
            await _fileSystem.File.WriteAllTextAsync(SettingsFilePath, json);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update settings");
        }
    }

    #endregion
}