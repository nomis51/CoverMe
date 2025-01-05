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

    #endregion

    #region Members

    private List<Project> TestsProjects { get; set; } = [];
    private string SelectedTestsProject { get; set; } = string.Empty;
    private List<CoverageNode> Nodes { get; set; } = [];
    private string FilterText { get; set; } = string.Empty;
    private bool IsRunningCoverage { get; set; }
    private bool IsGeneratingReport { get; set; }
    private bool IsRefreshing { get; set; }

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
        if (firstRender)
        {
            await RetrieveLastCoverage();
            await RetrieveTestsProjects();
            StateHasChanged();
        }
    }

    #endregion

    #region Private methods

    private async Task GenerateReport(bool detailed = false)
    {
        try
        {
            IsGeneratingReport = true;
            var projectSettings = await GetProjectSettings();
            var solution = new Solution(FileSystem, projectSettings.ProjectRootPath);

            var reportFolder = await CoverageService.GenerateReport(solution, detailed);
            if (string.IsNullOrEmpty(reportFolder)) return;

            IntellijService.SaveReport(projectSettings.ChannelId, reportFolder);
        }
        finally
        {
            IsGeneratingReport = false;
        }
    }

    private async Task Refresh()
    {
        IsRefreshing = true;
        await RetrieveTestsProjects();
        await RetrieveLastCoverage();
        IsRefreshing = false;
    }

    private async Task RetrieveLastCoverage()
    {
        IsRunningCoverage = true;
        var solution = await GetSolution();
        Nodes = await CoverageService.ParseLastCoverage(solution);
        IsRunningCoverage = false;
    }

    private async Task RunCoverage(bool noBuild = true)
    {
        try
        {
            IsRunningCoverage = true;
            var selectedProject = TestsProjects.FirstOrDefault(e => e.FilePath == SelectedTestsProject);
            if (selectedProject is null) return;

            var solution = await GetSolution();

            Nodes = await CoverageService.RunCoverage(solution, selectedProject, new CoverageOptions
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

    private async Task RetrieveTestsProjects()
    {
        var solution = await GetSolution();
        TestsProjects = CoverageService.GetTestsProjects(solution);

        if (string.IsNullOrEmpty(SelectedTestsProject) && TestsProjects.Count > 0)
        {
            SelectedTestsProject = TestsProjects[0].FilePath;
        }
    }

    #endregion
}