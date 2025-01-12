using CoverMe.Backend.Extensions.JavaScript;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;

namespace CoverMe.Backend.Layouts;

public partial class MainLayout : LayoutComponentBase
{
    #region Services

    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;

    #endregion

    #region Members

    private MudTheme Theme { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = new MudColor("#fff"),
        }
    };

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

        Theme.PaletteDark.Primary = settings.Theme.Colors.AccentColor;
    }

    #endregion
}