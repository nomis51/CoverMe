using CoverMe.Ipc.Models.Abstractions;

namespace CoverMe.Ipc.Abstractions;

public interface IIpcServer : IAsyncDisposable
{
    event EventHandler<IpcMessage>? MessageReceived;
    Task<IIpcChannel> CreateChannelAsync();
    Task<bool> RemoveChannelAsync(Guid id);
    Task<bool> RemoveAllChannelsAsync();
    Task SendMessage(Guid channelId, IpcMessage message);
}