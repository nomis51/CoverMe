using CoverMe.Backend.Core.Models.Settings;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CoverMe.Backend.Pages;

public partial class SettingsPage : ComponentBase
{
    #region Services

    [Inject]
    protected ISettingsService SettingsService { get; set; } = null!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Members

    private Settings Settings { get; set; } = new();
    private bool IsLoading { get; set; }

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        RetrieveSettings();
    }

    #endregion

    #region Private methods

    private void Save()
    {
        Task.Run(async () => { await SettingsService.UpdateSettingsAsync(Settings); });
    }

    private void RetrieveSettings()
    {
        IsLoading = true;
        Task.Run(async () =>
        {
            try
            {
                Settings = await SettingsService.GetSettingsAsync();
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