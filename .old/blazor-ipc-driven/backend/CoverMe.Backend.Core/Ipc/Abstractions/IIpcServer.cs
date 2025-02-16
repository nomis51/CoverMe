namespace CoverMe.Backend.Core.Ipc.Abstractions;

public interface IIpcServer : IDisposable
{
    string CreateChannel();
    bool RemoveChannel(string channelId);
    void RemoveAllChannels();
}