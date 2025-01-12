using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;

namespace CoverMe.Backend.Layouts;

public partial class MainLayout : LayoutComponentBase
{
    #region Members

    private MudTheme Theme { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = new MudColor("#fff"),
        }
    };

    #endregion
}