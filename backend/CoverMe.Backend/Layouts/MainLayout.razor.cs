using CoverMe.Backend.Extensions.JavaScript;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoverMe.Backend.Layouts;

public partial class MainLayout : LayoutComponentBase
{
    #region Services

    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await RetrieveIntellijTheme();
    }

    #endregion

    #region Private methods

    private async Task RetrieveIntellijTheme()
    {
        var settings = await JsRuntime.GetProjectSettings();
        if (settings is null) return;

    }

    #endregion
}