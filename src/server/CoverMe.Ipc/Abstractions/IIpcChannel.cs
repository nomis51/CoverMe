using CoverMe.Ipc.Models.Abstractions;

namespace CoverMe.Ipc.Abstractions;

public interface IIpcChannel : IDisposable
{
    event EventHandler<IpcMessage> MessageReceived;
    Guid Id { get; }
    Task SendMessageAsync(IpcMessage message);
    Task StartAsync(CancellationToken cancellationToken);
}