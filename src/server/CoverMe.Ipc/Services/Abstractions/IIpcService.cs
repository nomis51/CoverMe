using CoverMe.Ipc.Abstractions;
using CoverMe.Ipc.Models.Abstractions;

namespace CoverMe.Ipc.Services.Abstractions;

public interface IIpcService
{
    void SendMessage(IpcMessage message);
    Task<IIpcChannel> CreateChannelAsync();
}