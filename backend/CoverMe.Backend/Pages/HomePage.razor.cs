using CoverMe.Backend.Components.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Pages;

public partial class HomePage : AppComponentBase
{
    #region Services

    [Inject]
    protected ICoverageService CoverageService { get; set; } = null!;

    [Inject]
    protected IIntellijService IntellijService { get; set; } = null!;

    [Inject]
    protected ILogger<HomePage> Logger { get; set; } = null!;

    #endregion

    #region Members

    private List<Project> TestsProjects { get; set; } = [];
    private string SelectedTestsProject { get; set; } = string.Empty;
    private List<CoverageNode> Nodes { get; set; } = [];
    private string FilterText { get; set; } = string.Empty;
    private bool IsRunningCoverage { get; set; }
    private bool IsGeneratingReport { get; set; }
    private bool IsRefreshing { get; set; }
    private bool IsSaveMenuOpen { get; set; }
    public bool IsMoreMenuOpen { get; set; }

    #endregion

    #region Props

    private bool IsLoading => IsRunningCoverage || IsGeneratingReport || IsRefreshing;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await Refresh();
            StateHasChanged();
        }
    }

    #endregion

    #region Private methods

    private void ShowSettingsPage()
    {
        IsMoreMenuOpen = false;
        // TODO: navigate
    }

    private async Task GenerateReport(bool detailed = false)
    {
        IsSaveMenuOpen = false;

        if (!HasProjectSettings)
        {
            Logger.LogWarning("Can't generate report, probably running outside of Intellij");
            return;
        }

        try
        {
            IsGeneratingReport = true;
            var solution = new Solution(FileSystem, ProjectSettings!.ProjectRootPath);

            var reportFolder = await CoverageService.GenerateReport(solution, detailed);
            if (string.IsNullOrEmpty(reportFolder)) return;

            IntellijService.SaveReport(ProjectSettings.ChannelId, reportFolder);
        }
        finally
        {
            IsGeneratingReport = false;
        }
    }

    private async Task Refresh()
    {
        IsMoreMenuOpen = false;
        IsRefreshing = true;

        RetrieveTestsProjects();
        await RetrieveLastCoverage();

        _ = Task.Run(async () =>
        {
            await Task.Delay(500);
            IsRefreshing = false;
            await InvokeAsync(StateHasChanged);
        });
    }

    private async Task RetrieveLastCoverage()
    {
        if (!HasProjectSettings)
        {
            Logger.LogWarning("Can't retrieve last coverage, probably running outside of Intellij");
            return;
        }

        IsRunningCoverage = true;
        Nodes = await CoverageService.ParseLastCoverage(Solution!);
        IsRunningCoverage = false;
    }

    private async Task RunCoverage(bool noBuild = true)
    {
        if (!HasProjectSettings)
        {
            Logger.LogWarning("Can't run coverage, probably running outside of Intellij");
            return;
        }

        try
        {
            IsRunningCoverage = true;
            var selectedProject = TestsProjects.FirstOrDefault(e => e.FilePath == SelectedTestsProject);
            if (selectedProject is null) return;

            Nodes = await CoverageService.RunCoverage(Solution!, selectedProject, new CoverageOptions
            {
                Rebuild = !noBuild,
                HideAutoProperties = true,
            });
        }
        finally
        {
            IsRunningCoverage = false;
        }
    }

    private void RetrieveTestsProjects()
    {
        if (!HasProjectSettings)
        {
            Logger.LogWarning("Can't retrieve tests projects, probably running outside of Intellij");
            return;
        }

        TestsProjects = CoverageService.GetTestsProjects(Solution!);

        if (string.IsNullOrEmpty(SelectedTestsProject) && TestsProjects.Count > 0)
        {
            SelectedTestsProject = TestsProjects[0].FilePath;
        }
    }

    #endregion
}