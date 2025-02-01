using CoverMe.Backend.Components.Abstractions;
using CoverMe.Backend.Core.Enums.Coverage;
using CoverMe.Backend.Core.Models.Settings;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Components;

public partial class CoverageTableRow : AppComponentBase
{
    #region Parameters

    [Parameter]
    public string Symbol { get; set; } = string.Empty;

    [Parameter]
    public int Coverage { get; set; }

    [Parameter]
    public int CoveredStatements { get; set; }

    [Parameter]
    public int TotalStatements { get; set; }

    [Parameter]
    public bool CanBeExpanded { get; set; }

    [Parameter]
    public int Level { get; set; }

    [Parameter]
    public CoverageNodeIcon Icon { get; set; }

    [Parameter]
    public string FilePath { get; set; } = string.Empty;

    [Parameter]
    public int Line { get; set; }

    [Parameter]
    public EventCallback OnExpand { get; set; } = EventCallback.Empty;

    [Parameter]
    public bool IsExpanded { get; set; }

    #endregion

    #region Services

    [Inject]
    protected ILogger<CoverageTableRow> Logger { get; set; } = null!;

    [Inject]
    protected IIntellijService IntellijService { get; set; } = null!;

    [Inject]
    protected ISettingsService SettingsService { get; set; } = null!;

    #endregion

    #region Members

    private Settings Settings { get; set; } = null!;

    #endregion

    #region Props

    private string BarColor => Coverage switch
    {
        > 80 => "green",
        > 70 => "#c1c131",
        > 50 => "#b38022",
        _ => "#c13333"
    };

    private string FontColor => Coverage switch
    {
        _ => "white"
    };

    private bool HasFilePath => !string.IsNullOrEmpty(FilePath);

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        await RetrieveSettings();
    }

    #endregion

    #region Private methods

    private async Task RetrieveSettings()
    {
        Settings = await SettingsService.GetSettingsAsync();
    }

    private void OpenFileAtLine()
    {
        if (!HasFilePath) return;
        if (!HasProjectSettings)
        {
            Logger.LogWarning("Can't open file at line, probably running outside of Intellij");
            return;
        }

        IntellijService.OpenFileAtLine(ProjectSettings!.ChannelId, FilePath, Line);
    }

    #endregion
}