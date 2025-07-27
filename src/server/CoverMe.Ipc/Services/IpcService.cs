using CoverMe.Ipc.Abstractions;
using CoverMe.Ipc.Models.Abstractions;
using CoverMe.Ipc.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Ipc.Services;

public class IpcService : IIpcService
{
    #region Members

    private readonly IIpcServer _ipcServer;
    private readonly ILogger<IpcService> _logger;

    #endregion

    #region Constructors

    public IpcService(ILogger<IpcService> logger, IIpcServer ipcServer)
    {
        _logger = logger;
        _ipcServer = ipcServer;

        _ipcServer.MessageReceived += MessageReceived;
    }

    #endregion

    #region Public methods

    public void SendMessage(IpcMessage message)
    {
        // TODO: Send message
    }

    public Task<IIpcChannel> CreateChannelAsync()
    {
        return _ipcServer.CreateChannelAsync();
    }

    #endregion

    #region Private methods

    private Task HandleMessageAsync(IIpcChannel channel, IpcMessage message)
    {
        try
        {
            return message.MessageType switch
            {
                _ => throw new InvalidOperationException(),
            };
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unexpected IPC message type: {Message}", message);
        }

        return Task.CompletedTask;
    }

    private void MessageReceived(object? sender, IpcMessage e)
    {
        HandleMessageAsync((IIpcChannel)sender!, e);
    }

    #endregion
}