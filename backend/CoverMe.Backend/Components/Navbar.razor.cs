using CoverMe.Backend.Components.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using TaskExtensions = CoverMe.Backend.Extensions.TaskExtensions;

namespace CoverMe.Backend.Components;

public partial class Navbar : AppComponentBase
{
    #region Parameters

    [Parameter]
    public EventCallback<(CoverageOptions Options, Project Project)> OnRunCoverage { get; set; } =
        EventCallback<(CoverageOptions Options, Project Project)>.Empty;

    [Parameter]
    public EventCallback OnRefreshCoverage { get; set; } = EventCallback.Empty;

    [Parameter]
    public string FilterText { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> OnFilterTextChanged { get; set; } = EventCallback<string>.Empty;

    #endregion

    #region Services

    [Inject]
    protected ICoverageService CoverageService { get; set; } = null!;

    [Inject]
    protected ILogger<Navbar> Logger { get; set; } = null!;

    [Inject]
    protected IIntellijService IntellijService { get; set; } = null!;

    #endregion

    #region Members

    public List<Project> TestsProjects { get; set; } = [];
    private string SelectedTestsProject { get; set; } = string.Empty;
    private bool IsRunningCoverage { get; set; }
    private bool IsGeneratingReport { get; set; }
    private bool IsRefreshing { get; set; }
    private bool IsSaveMenuOpen { get; set; }
    public bool IsMoreMenuOpen { get; set; }

    #endregion

    #region Props

    private bool IsLoading => IsRunningCoverage || IsGeneratingReport || IsRefreshing;

    #endregion

    #region Lifecycle

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Task.Run(async () =>
            {
                await TaskExtensions.WaitUntil(
                    () => HasProjectSettings,
                    async () => await InvokeAsync(async () => await Refresh())
                );
            });
        }
    }

    #endregion

    #region Private methods

    private async Task RunCoverage(bool noBuild = true)
    {
        var project = TestsProjects.FirstOrDefault(e => e.FilePath == SelectedTestsProject);
        if (project is null) return;

        await OnRunCoverage.InvokeAsync((new CoverageOptions
        {
            Rebuild = !noBuild,
            HideAutoProperties = true,
            Filter = FilterText,
            ParserExcluded = "",
        }, project));
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
        await OnRefreshCoverage.InvokeAsync();

        _ = Task.Run(async () =>
        {
            await Task.Delay(500);
            IsRefreshing = false;
            await InvokeAsync(StateHasChanged);
        });
    }

    #endregion
}