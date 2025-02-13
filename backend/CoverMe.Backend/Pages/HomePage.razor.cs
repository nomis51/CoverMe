using CoverMe.Backend.Components;
using CoverMe.Backend.Components.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using TaskExtensions = CoverMe.Backend.Extensions.TaskExtensions;

namespace CoverMe.Backend.Pages;

public partial class HomePage : AppComponentBase
{
    #region Services

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    protected ICoverageService CoverageService { get; set; } = null!;


    [Inject]
    protected IIntellijService IntellijService { get; set; } = null!;

    #endregion

    #region Members

    private CoverageTable CoverageTableRef { get; set; } = null!;
    private bool IsLoading { get; set; }
    private string FilterText { get; set; } = string.Empty;
    private string SelectedProjectFilePath { get; set; } = string.Empty;
    private List<Project> Projects { get; set; } = [];
    private bool IsSaveMenuOpen { get; set; }
    private bool IsMoreMenuOpen { get; set; }
    private List<CoverageNode> Nodes { get; set; } = [];

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

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Refresh();
        }
    }

    #endregion

    #region Private methods

    private void UpdateFilterText(string value)
    {
        FilterText = value;
        Refresh();
    }

    private void RunCoverage(bool noBuild)
    {
        if (Solution is null) return;

        var project = Projects.FirstOrDefault(e => e.FilePath == SelectedProjectFilePath);
        if (project is null) return;

        IsLoading = true;

        Task.Run(async () =>
        {
            try
            {
                Nodes = await CoverageService.RunCoverage(
                    Solution,
                    project,
                    new CoverageOptions
                    {
                        Rebuild = !noBuild,
                        Filter = FilterText,
                    }
                );
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        });
    }

    private void GenerateReport(bool detailed)
    {
        IsSaveMenuOpen = false;

        if (Solution is null) return;
        if (!HasProjectSettings) return;

        IsLoading = true;
        Task.Run(async () =>
        {
            try
            {
                var outputFolder = await CoverageService.GenerateReport(Solution, detailed);
                if (string.IsNullOrEmpty(outputFolder)) return;

                IntellijService.SaveReport(ProjectSettings!.ChannelId, outputFolder);
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        });
    }

    private void GotoSettings()
    {
        NavigationManager.NavigateTo("/settings");
    }

    private void Refresh()
    {
        _ = TaskExtensions.WaitUntil(() => HasProjectSettings, () =>
        {
            RetrieveProjects();
            RetrieveLastCoverage();
        });
    }

    private void RetrieveProjects()
    {
        if (Solution is null) return;

        IsLoading = true;
        Task.Run(async () =>
        {
            try
            {
                Projects = CoverageService.GetTestsProjects(Solution);

                if (Projects.Count > 0 && string.IsNullOrEmpty(SelectedProjectFilePath))
                {
                    SelectedProjectFilePath = Projects[0].FilePath;
                }
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        });
    }

    private void RetrieveLastCoverage()
    {
        if (Solution is null) return;

        IsLoading = true;

        Task.Run(async () =>
        {
            try
            {
                Nodes = await CoverageService.ParseLastCoverage(
                    Solution,
                    new CoverageOptions
                    {
                        Filter = FilterText,
                        Rebuild = false
                    }
                );
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        });
    }

    #endregion
}