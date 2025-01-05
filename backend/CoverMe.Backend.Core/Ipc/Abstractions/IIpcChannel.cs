using CoverMe.Backend.Core.Models.Ipc.Abstractions;

namespace CoverMe.Backend.Core.Ipc.Abstractions;

public interface IIpcChannel : IDisposable
{
    delegate void MessageReceivedEvent(IpcMessage message);

    event MessageReceivedEvent? MessageReceived;
    string Id { get; }
    Task SendMessageAsync(IpcMessage message);
    Task StartAsync(CancellationToken cancellationToken);
}