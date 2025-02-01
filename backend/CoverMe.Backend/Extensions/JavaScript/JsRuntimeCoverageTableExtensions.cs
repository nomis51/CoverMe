using CoverMe.Backend.Models.JavaScript.Intellij;
using Microsoft.JSInterop;

namespace CoverMe.Backend.Extensions.JavaScript;

public static class JsRuntimeCoverageTableExtensions
{
    #region Constants

    private const string Namespace = "coverageTable";

    #endregion

    #region Public methods

    public static async Task InitializeResizableTable(this IJSRuntime jsRuntime, string id)
    {
        await jsRuntime.InvokeVoidAsync(
            $"{JsRuntimeExtensions.GlobalNamespace}.{Namespace}.initialize",
            new { id }
        );
    }

    public static async Task DisposeResizableTable(this IJSRuntime jsRuntime)
    {
        await jsRuntime.InvokeVoidAsync(
            $"{JsRuntimeExtensions.GlobalNamespace}.{Namespace}.dispose"
        );
    }

    #endregion
}