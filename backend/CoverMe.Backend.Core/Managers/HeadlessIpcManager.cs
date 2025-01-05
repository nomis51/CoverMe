using CoverMe.Backend.Core.Enums.Ipc;
using CoverMe.Backend.Core.Ipc.Abstractions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Backend.Core.Managers;

public class HeadlessIpcManager : IIpcManager
{
    #region Events

    public event IIpcManager.MessageToSendRequestedEvent? MessageToSendRequested;

    #endregion

    #region Members

    private readonly ILogger<HeadlessIpcManager> _logger;

    #endregion

    #region Constructors

    public HeadlessIpcManager(ILogger<HeadlessIpcManager> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Public methods

    public Task HandleMessageAsync(IpcMessage message, IIpcChannel channel)
    {
        switch (message.MessageType)
        {
            case IpcMessageType.GetFileLineCoverage:
                _logger.LogInformation("Headless IPC: Getting file at line coverage {Message}", message);
                break;

            case IpcMessageType.OpenFileAtLine:
            default:
                _logger.LogInformation("Headless IPC: Unknown message type {MessageType}", message.MessageType);
                break;
        }

        return Task.CompletedTask;
    }

    public void SendMessage(IpcMessage message)
    {
        _logger.LogInformation("Headless IPC: Sending message {Message}", message);
    }

    #endregion
}