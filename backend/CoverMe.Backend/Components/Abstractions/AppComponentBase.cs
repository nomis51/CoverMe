using System.IO.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.AppSettings;
using CoverMe.Backend.Extensions.JavaScript;
using CoverMe.Backend.Models.JavaScript.Intellij;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoverMe.Backend.Components.Abstractions;

public abstract class AppComponentBase : ComponentBase
{
    #region Services

    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    protected AppSettings AppSettings { get; set; } = null!;

    [Inject]
    protected IFileSystem FileSystem { get; set; } = null!;

    #endregion

    #region Props

    private static bool IsHeadless => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Headless";

    #endregion

    #region Protected methods

    protected async Task<ProjectSettings> GetProjectSettings()
    {
        return IsHeadless
            ? new ProjectSettings
            {
                ProjectRootPath = Path.GetFullPath(AppSettings.Intellij.ProjectRootPath),
                ChannelId = Guid.NewGuid().ToString("N")
            }
            : await JsRuntime.GetProjectSettings();
    }

    protected async Task<Solution> GetSolution()
    {
        var settings = await GetProjectSettings();
        return new Solution(FileSystem, settings.ProjectRootPath);
    }

    #endregion
}