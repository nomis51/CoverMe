using CoverMe.Backend.Models.JavaScript.Intellij;
using Microsoft.JSInterop;

namespace CoverMe.Backend.Extensions.JavaScript;

public static class JsRuntimeIntellijExtensions
{
    #region Constants

    private const string Namespace = "intellij";

    #endregion

    #region Public methods

    public static async Task<ProjectSettings?> GetProjectSettings(this IJSRuntime jsRuntime)
    {
        return await jsRuntime.InvokeAsync<ProjectSettings>(
            $"{JsRuntimeExtensions.GlobalNamespace}.{Namespace}.getProjectSettings"
        );
    }

    #endregion
}