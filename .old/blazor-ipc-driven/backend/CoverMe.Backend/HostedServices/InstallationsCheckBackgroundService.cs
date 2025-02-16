using CoverMe.Backend.Core.Helpers.Abstractions;

namespace CoverMe.Backend.HostedServices;

public class InstallationsCheckBackgroundService : BackgroundService
{
    #region Members

    private readonly ILogger<InstallationsCheckBackgroundService> _logger;
    private readonly IProcessHelper _processHelper;

    #endregion

    #region Constructors

    public InstallationsCheckBackgroundService(
        IProcessHelper processHelper,
        ILogger<InstallationsCheckBackgroundService> logger
    )
    {
        _processHelper = processHelper;
        _logger = logger;
    }

    #endregion

    #region Public methods

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        VerifyDotCoverCliInstallation();
        VerifyReportGeneratorInstallation();
        return Task.CompletedTask;
    }

    #endregion

    #region Private methods

    private Task VerifyDotCoverCliInstallation()
    {
        _logger.LogInformation("Verifying DotCover CLI installation");
        return _processHelper.EnsureDotCoverCliInstalled();
    }

    private Task VerifyReportGeneratorInstallation()
    {
        _logger.LogInformation("Verifying Report Generator installation");
        return _processHelper.EnsureReportGeneratorInstalled();
    }

    #endregion
}