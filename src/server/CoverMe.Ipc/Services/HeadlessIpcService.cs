using CoverMe.Ipc.Abstractions;
using CoverMe.Ipc.Models.Abstractions;
using CoverMe.Ipc.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Ipc.Services;

public class HeadlessIpcService : IIpcService
{
    public void SendMessage(IpcMessage message)
    {
        throw new NotImplementedException();
    }

    public Task<IIpcChannel> CreateChannelAsync()
    {
        throw new NotImplementedException();
    }
}