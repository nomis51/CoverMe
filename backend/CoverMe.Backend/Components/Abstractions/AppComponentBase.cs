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

    #region Members

    protected ProjectSettings? ProjectSettings { get; private set; }
    protected Solution? Solution { get; private set; }

    #endregion

    #region Props

    private static bool IsHeadless => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Headless";
    protected bool HasProjectSettings => ProjectSettings is not null;

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ProjectSettings = await GetProjectSettings();
            if (ProjectSettings is null) return;

            Solution = new Solution(FileSystem, ProjectSettings.ProjectRootPath);
        }
    }

    #endregion

    #region Private methods

    private async Task<ProjectSettings?> GetProjectSettings()
    {
        return IsHeadless
            ? new ProjectSettings
            {
                ProjectRootPath = Path.GetFullPath(AppSettings.Intellij.ProjectRootPath),
                ChannelId = Guid.NewGuid().ToString("N"),
            }
            : await JsRuntime.GetProjectSettings();
    }

    #endregion
}