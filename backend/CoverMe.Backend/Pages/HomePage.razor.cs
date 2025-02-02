using CoverMe.Backend.Components;
using CoverMe.Backend.Components.Abstractions;

namespace CoverMe.Backend.Pages;

public partial class HomePage : AppComponentBase
{
    #region Members

    private CoverageTable CoverageTableRef { get; set; } = null!;
    private bool IsLoading { get; set; }
    private string FilterText { get; set; } = string.Empty;

    #endregion
}