using CoverMe.Ipc;
using CoverMe.Ipc.Abstractions;
using CoverMe.Ipc.Services;
using CoverMe.Ipc.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoverMe.Core.Extensions;

public static class ServiceCollectionExtensions
{
    #region Public methods

    public static IServiceCollection AddAppServices(this IServiceCollection services, IWebHostEnvironment environment)
    {
        RegisterIpc(services, environment);
        return services;
    }

    #endregion

    #region Private methds

    private static void RegisterIpc(IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSingleton<IIpcServer, IpcServer>();

        if (environment.IsEnvironment("Headless"))
        {
            services.AddScoped<IIpcService, HeadlessIpcService>();
        }
        else
        {
            services.AddScoped<IIpcService, IpcService>();
        }
    }

    #endregion
}