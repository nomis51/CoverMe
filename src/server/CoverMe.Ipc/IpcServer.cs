using CoverMe.Ipc.Abstractions;
using CoverMe.Ipc.Exceptions;
using CoverMe.Ipc.Models.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Ipc;

public class IpcServer : IIpcServer
{
    #region Constants

    private static readonly TimeSpan AutoExitDelay = TimeSpan.FromSeconds(10);

    #endregion

    #region Events

    public event EventHandler<IpcMessage>? MessageReceived;

    #endregion

    #region Members

    private readonly ILogger<IpcServer> _logger;
    private readonly ILogger<IpcChannel> _loggerChannel;
    private readonly Dictionary<Guid, IIpcChannel> _channels = [];
    private readonly SemaphoreSlim _channelsLock = new(1, 1);
    private readonly CancellationTokenSource _channelsCancellationTokenSource = new();
    private Task? _exitTask;
    private CancellationTokenSource? _exitCancellationTokenSource;

    #endregion

    #region Constructors

    public IpcServer(ILogger<IpcServer> logger, ILogger<IpcChannel> loggerChannel)
    {
        _logger = logger;
        _loggerChannel = loggerChannel;
    }

    #endregion

    #region Public methods

    public async Task<IIpcChannel> CreateChannelAsync()
    {
        _logger.LogTrace("IPC: Creating channel");
        ResetAutoExit();

        await _channelsLock.WaitAsync(_channelsCancellationTokenSource.Token);

        try
        {
            var id = Guid.NewGuid();
            while (_channels.ContainsKey(id))
            {
                id = Guid.NewGuid();
            }

            var channel = new IpcChannel(id, _loggerChannel);

            var ok = _channels.TryAdd(id, channel);
            if (!ok)
            {
                _logger.LogWarning("IPC: Failed to add channel {Id}", id);
                throw IpcChannelCreationFailedException.Static;
            }

            channel.MessageReceived += Channel_OnMessageReceived;
            StartChannel(channel);

            _logger.LogTrace("IPC: Created channel {Id}", id);
            return channel;
        }
        finally
        {
            _channelsLock.Release();
        }
    }

    public async Task<bool> RemoveChannelAsync(Guid id)
    {
        _logger.LogTrace("IPC: Removing channel {Id}", id);
        await _channelsLock.WaitAsync(_channelsCancellationTokenSource.Token);

        try
        {
            var ok = _channels.Remove(id, out var channel);
            if (!ok)
            {
                _logger.LogWarning("IPC: Failed to remove channel {Id}", id);
                return false;
            }

            channel!.Dispose();
            _logger.LogTrace("IPC: Removed channel {Id}", id);

            CheckIfExitRequired();

            return true;
        }
        finally
        {
            _channelsLock.Release();
        }
    }

    public async Task<bool> RemoveAllChannelsAsync()
    {
        _logger.LogTrace("IPC: Removing all channels");
        await _channelsLock.WaitAsync(_channelsCancellationTokenSource.Token);

        try
        {
            foreach (var (id, _) in _channels)
            {
                if (!await RemoveChannelAsync(id)) return false;
            }
        }
        finally
        {
            _channelsLock.Release();
        }

        return true;
    }

    public async Task SendMessage(Guid channelId, IpcMessage message)
    {
        _logger.LogTrace("IPC: Sending message {Message} to channel {Id}", message, channelId);
        await _channelsLock.WaitAsync(_channelsCancellationTokenSource.Token);

        try
        {
            if (!_channels.TryGetValue(channelId, out var channel)) return;

            await channel.SendMessageAsync(message);
        }
        finally
        {
            _channelsLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await _channelsCancellationTokenSource.CancelAsync();
        await RemoveAllChannelsAsync();
    }

    #endregion

    #region Private methods

    private void ResetAutoExit()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production") return;
        if (_exitTask is null || _exitCancellationTokenSource is null) return;

        _logger.LogTrace("Resetting auto exit");
        _exitCancellationTokenSource.Cancel();
        _exitCancellationTokenSource.Dispose();
        _exitTask = null;
    }

    private void CheckIfExitRequired()
    {
        _logger.LogTrace("No more channels, checking if exit is required");
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production") return;
        if (_channels.Count != 0 || _exitTask is not null) return;

        _exitCancellationTokenSource = new CancellationTokenSource();
        _exitTask = Task.Run(async () =>
        {
            _logger.LogTrace("No more channels, exiting in {Time}s", AutoExitDelay / 1000);
            await Task.Delay(AutoExitDelay, _exitCancellationTokenSource.Token);
            if (_exitCancellationTokenSource.IsCancellationRequested) return;

            _logger.LogTrace("No more channels, exiting");
            Environment.Exit(0);
        }, _exitCancellationTokenSource.Token);
    }

    private void StartChannel(IIpcChannel channel)
    {
        _logger.LogTrace("IPC: Starting channel {Id}", channel.Id);
        Task.Run(async () =>
        {
            await channel.StartAsync(_channelsCancellationTokenSource.Token);
            await RemoveChannelAsync(channel.Id);
        }, _channelsCancellationTokenSource.Token);
    }

    private void Channel_OnMessageReceived(object? sender, IpcMessage message)
    {
        if (!_channels.TryGetValue(message.ChannelId, out var channel)) return;

        _logger.LogTrace("IPC [{ChannelId}]: Received message {Message}", message.ChannelId, message);
        MessageReceived?.Invoke(this, message);
    }

    private void IpcServiceOnMessageToSendRequested(IpcMessage message)
    {
        if (!_channels.TryGetValue(message.ChannelId, out var channel)) return;

        _logger.LogTrace("IPC [{ChannelId}]: Sending message {Message}", message.ChannelId, message);
        channel.SendMessageAsync(message);
    }

    #endregion
}