using CoverMe.Backend.Core.Ipc.Abstractions;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;

namespace CoverMe.Backend.Core.Managers.Abstractions;

public interface IIpcManager
{
    delegate void MessageToSendRequestedEvent(IpcMessage message);

    event MessageToSendRequestedEvent? MessageToSendRequested;
    Task HandleMessageAsync(IpcMessage message, IIpcChannel channel);
    void SendMessage(IpcMessage message);
}