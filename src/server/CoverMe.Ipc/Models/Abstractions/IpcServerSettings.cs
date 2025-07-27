using CoverMe.Ipc.Abstractions;

namespace CoverMe.Ipc.Models.Abstractions;

public class IpcServerSettings
{
    public string Name { get; }
    public IIpcServer IpcServer { get; }
    public Task Task { get; set; } = null!;
    public CancellationTokenSource CancellationToken { get; } = new();

    public IpcServerSettings(string name, IIpcServer ipcServer)
    {
        Name = name;
        IpcServer = ipcServer;
    }
}