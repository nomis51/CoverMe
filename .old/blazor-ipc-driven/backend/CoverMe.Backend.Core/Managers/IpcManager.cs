using System.Text.Json;
using CoverMe.Backend.Core.Enums.Ipc;
using CoverMe.Backend.Core.Extensions;
using CoverMe.Backend.Core.Ipc.Abstractions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models.Ipc;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using CoverMe.Backend.Core.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Backend.Core.Managers;

public class IpcManager : IIpcManager
{
    #region Events

    public delegate void MessageToSendRequestedEvent(IpcMessage message);

    public event IIpcManager.MessageToSendRequestedEvent? MessageToSendRequested;

    #endregion

    #region Members

    private readonly ILogger<IpcManager> _logger;
    private readonly ICoverageService _coverageService;

    #endregion

    #region Constructors

    public IpcManager(ILogger<IpcManager> logger, ICoverageService coverageService)
    {
        _logger = logger;
        _coverageService = coverageService;
    }

    #endregion

    #region Public methods

    public Task HandleMessageAsync(IpcMessage message, IIpcChannel channel)
    {
        try
        {
            return message.MessageType switch
            {
                IpcMessageType.GetFileLineCoverage => HandleGetFileLineCoverage(message, channel),
                IpcMessageType.OpenFileAtLine => throw new InvalidOperationException(),
                _ => throw new InvalidOperationException(),
            };
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unexpected IPC message type: {Message}", message);
        }

        return Task.CompletedTask;
    }

    public void SendMessage(IpcMessage message)
    {
        MessageToSendRequested?.Invoke(message);
    }

    #endregion

    #region Private methods

    private async Task HandleGetFileLineCoverage(IpcMessage message, IIpcChannel channel)
    {
        var request = message.GetData<GetFileLineCoverageRequest>();
        var isLineCovered = await _coverageService.IsLineCovered(
            request.ProjectRootPath,
            request.FilePath,
            request.Line
        );
        await channel.SendMessageAsync(
            new IpcMessage(
                message.Id,
                message.ChannelId,
                IpcMessageType.GetFileLineCoverage.Description(),
                JsonSerializer.Serialize(isLineCovered)
            )
        );
    }

    #endregion
}