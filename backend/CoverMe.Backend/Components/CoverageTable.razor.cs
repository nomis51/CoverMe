using CoverMe.Backend.Components.Abstractions;
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
    public string FilterText { get; set; } = string.Empty;

    [Parameter]
    public bool IsLoading { get; set; }

    #endregion

    #region Services

    [Inject]
    protected ISettingsService SettingsService { get; set; } = null!;

    [Inject]
    protected ICoverageService CoverageService { get; set; } = null!;

    [Inject]
    protected ILogger<CoverageTable> Logger { get; set; } = null!;

    #endregion

    #region Members

    private Settings Settings { get; set; } = null!;

    #endregion

    #region Props

    private List<CoverageNode> FilteredNodes
    {
        get
        {
            if (string.IsNullOrEmpty(FilterText)) return Nodes;

            var result = new List<CoverageNode>();
            var matchedLevels = new HashSet<int>();

            for (var i = Nodes.Count - 1; i >= 0; --i)
            {
                var node = Nodes[i];

                if (node.Symbol.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
                    !Nodes.Any(x => x.Level == node.Level + 1 &&
                                    result.Contains(x)))
                {
                    matchedLevels.Add(node.Level);
                    result.Add(node);
                }
                else if (matchedLevels.Contains(node.Level + 1))
                {
                    matchedLevels.Add(node.Level);
                    result.Add(node);
                }
            }

            result.Reverse();
            return result;
        }
    }

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

    private async Task RunCoverage(bool noBuild = true)
    {
        if (!HasProjectSettings)
        {
            Logger.LogWarning("Can't run coverage, probably running outside of Intellij");
            return;
        }

        var selectedProject = TestsProjects.FirstOrDefault(e => e.FilePath == SelectedTestsProject);
        if (selectedProject is null) return;

        Nodes = await CoverageService.RunCoverage(Solution!, selectedProject, new CoverageOptions
        {
            Rebuild = !noBuild,
            HideAutoProperties = true,
        });
    }

    private async Task RetrieveLastCoverage()
    {
        if (!HasProjectSettings)
        {
            Logger.LogWarning("Can't retrieve last coverage, probably running outside of Intellij");
            return;
        }

        Nodes = await CoverageService.ParseLastCoverage(Solution!);
    }

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