using CoverMe.Backend.Core.Models.Settings;

namespace CoverMe.Backend.Core.Services.Abstractions;

public interface ISettingsService
{
    Task<Settings> GetSettingsAsync();
    Task UpdateSettingsAsync(Settings settings);
}