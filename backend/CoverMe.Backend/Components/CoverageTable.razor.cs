using CoverMe.Backend.Components.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;
using CoverMe.Backend.Core.Models.Settings;
using CoverMe.Backend.Core.Services.Abstractions;
using CoverMe.Backend.Extensions.JavaScript;
using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Components;

public partial class CoverageTable : AppComponentBase, IDisposable
{
    #region Constants

    private readonly string _tableId = Guid.NewGuid().ToString("N");

    #endregion

    #region Parameters

    [Parameter]

    public List<CoverageNode> Nodes { get; set; } = [];

    [Parameter]
    public bool IsLoading { get; set; }

    #endregion

    #region Services

    [Inject]
    protected ISettingsService SettingsService { get; set; } = null!;

    #endregion

    #region Members

    private Settings Settings { get; set; } = null!;

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        await RetrieveSettings();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await JsRuntime.InitializeResizableTable(_tableId);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _ = JsRuntime.DisposeResizableTable();
    }

    #endregion

    #region Private methods

    private async Task RetrieveSettings()
    {
        Settings = await SettingsService.GetSettingsAsync();
    }

    private void ToggleExpand(int index)
    {
        Nodes[index].IsExpanded = !Nodes[index].IsExpanded;
        if (Nodes[index].IsExpanded) return;

        for (var i = index + 1; i < Nodes.Count; ++i)
        {
            Nodes[i].IsExpanded = Nodes[index].IsExpanded;
        }
    }

    private bool CanBeExpanded(int index)
    {
        if (index + 1 >= Nodes.Count) return false;

        return Nodes[index + 1].Level > Nodes[index].Level;
    }

    private bool IsVisible(int index)
    {
        if (index == 0) return true;

        for (var i = index - 1; i >= 0; --i)
        {
            if (Nodes[i].Level >= Nodes[index].Level) continue;

            return Nodes[i].IsExpanded;
        }

        return true;
    }

    #endregion
}