using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Components;

public partial class IconButton : ComponentBase
{
    #region Parameters

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public EventCallback OnClick { get; set; } = EventCallback.Empty;

    [Parameter]
    public bool Disabled { get; set; }

    #endregion
}