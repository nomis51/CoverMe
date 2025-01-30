using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Components.Icons.Abstractions;

public partial class Icon : ComponentBase
{
    #region Parameters

    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    #endregion
}