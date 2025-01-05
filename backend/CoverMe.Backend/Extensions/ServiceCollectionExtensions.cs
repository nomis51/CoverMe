using System.IO.Abstractions;
using CoverMe.Backend.Core.Helpers;
using CoverMe.Backend.Core.Helpers.Abstractions;
using CoverMe.Backend.Core.Ipc;
using CoverMe.Backend.Core.Ipc.Abstractions;
using CoverMe.Backend.Core.Managers;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models.AppSettings;
using CoverMe.Backend.Core.Services;
using CoverMe.Backend.Core.Services.Abstractions;
using CoverMe.Backend.HostedServices;
using MudBlazor.Services;
using Serilog;

namespace CoverMe.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    #region Props

    private static bool IsHeadless => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Headless";

    #endregion

    #region Public methods

    public static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<InstallationsCheckBackgroundService>();
        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddSerilog();
        services.AddMudServices();
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddSingleton<AppSettings>();
        services.AddSingleton<IProcessHelper, ProcessHelper>();
        services.AddSingleton<ICoverageService, CoverageService>();
        services.AddScoped<IIntellijService, IntellijService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        return services;
    }

    public static IServiceCollection AddIpc(this IServiceCollection services)
    {
        services.AddSingleton<IIpcServer, IpcServer>();
        if (IsHeadless)
        {
            services.AddSingleton<IIpcManager, HeadlessIpcManager>();
        }
        else
        {
            services.AddSingleton<IIpcManager, IpcManager>();
        }

        return services;
    }

    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(ICacheManager<>), typeof(CacheManager<>));
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services;
    }

    #endregion
}