using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Components;

public partial class IconButton : ComponentBase
{
    #region Parameters

    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    #endregion
}