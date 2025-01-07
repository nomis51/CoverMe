﻿using System.Collections.Concurrent;
using CoverMe.Backend.Core.Ipc.Abstractions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Backend.Core.Ipc;

public class IpcServer : IIpcServer
{
    #region Constants

    private int AutoExitDelay = 10_000;

    #endregion

    #region Members

    private readonly ILogger<IpcServer> _logger;
    private readonly ILogger<IpcChannel> _loggerChannel;
    private readonly ConcurrentDictionary<string, IIpcChannel> _channels = [];
    private readonly IIpcManager _ipcManager;
    private readonly CancellationTokenSource _channelsCancellationTokenSource = new();
    private Task? _exitTask;
    private CancellationTokenSource? _exitCancellationTokenSource;

    #endregion

    #region Constructors

    public IpcServer(ILogger<IpcServer> logger, ILogger<IpcChannel> loggerChannel, IIpcManager ipcManager)
    {
        _logger = logger;
        _loggerChannel = loggerChannel;
        _ipcManager = ipcManager;
        _ipcManager.MessageToSendRequested += IpcManager_OnMessageToSendRequested;
    }

    #endregion

    #region Public methods

    public string CreateChannel()
    {
        ResetAutoExit();

        var id = Guid.NewGuid().ToString("N");
        while (_channels.ContainsKey(id))
        {
            id = Guid.NewGuid().ToString("N");
        }

        var channel = new IpcChannel(id, _loggerChannel);

        var ok = _channels.TryAdd(id, channel);
        if (!ok) return string.Empty;

        channel.MessageReceived += Channel_OnMessageReceived;
        StartChannel(channel);

        _logger.LogInformation("IPC: Created channel {Id}", id);
        return id;
    }

    public bool RemoveChannel(string channelId)
    {
        var ok = _channels.TryRemove(channelId, out var channel);
        if (!ok) return false;

        channel!.Dispose();
        _logger.LogInformation("IPC: Removed channel {Id}", channelId);

        CheckIfExitRequired();

        return true;
    }

    public void RemoveAllChannels()
    {
        foreach (var (id, _) in _channels)
        {
            RemoveChannel(id);
        }
    }

    public void Dispose()
    {
        _channelsCancellationTokenSource.Cancel();
        RemoveAllChannels();
    }

    #endregion

    #region Private methods

    private void ResetAutoExit()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production") return;
        if (_exitTask is null || _exitCancellationTokenSource is null) return;

        _exitCancellationTokenSource.Cancel();
        _exitTask = null;
    }

    private void CheckIfExitRequired()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production") return;
        if (_channels.Count != 0 || _exitTask is not null) return;

        _exitCancellationTokenSource = new CancellationTokenSource();
        _exitTask = Task.Run(async () =>
        {
            await Task.Delay(AutoExitDelay, _exitCancellationTokenSource.Token);
            if (_exitCancellationTokenSource.IsCancellationRequested) return;

            Environment.Exit(0);
        }, _exitCancellationTokenSource.Token);
    }

    private void StartChannel(IIpcChannel channel)
    {
        Task.Run(async () =>
        {
            await channel.StartAsync(_channelsCancellationTokenSource.Token);
            RemoveChannel(channel.Id);
        }, _channelsCancellationTokenSource.Token);
    }

    private void Channel_OnMessageReceived(IpcMessage message)
    {
        if (!_channels.TryGetValue(message.ChannelId, out var channel)) return;

        _logger.LogInformation("IPC [{ChannelId}]: Received message {Message}", message.ChannelId, message);
        _ipcManager.HandleMessageAsync(message, channel);
    }

    private void IpcManager_OnMessageToSendRequested(IpcMessage message)
    {
        if (!_channels.TryGetValue(message.ChannelId, out var channel)) return;

        _logger.LogInformation("IPC [{ChannelId}]: Sending message {Message}", message.ChannelId, message);
        channel.SendMessageAsync(message);
    }

    #endregion
}