using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Components.Icons.Abstractions;

public class IconBase : ComponentBase
{
    #region Parameters

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    #endregion
}