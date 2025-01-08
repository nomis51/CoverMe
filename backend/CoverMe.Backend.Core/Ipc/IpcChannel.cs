using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using CoverMe.Backend.Core.Ipc.Abstractions;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoverMe.Backend.Core.Ipc;

public class IpcChannel : IIpcChannel
{
    #region Events

    public delegate void MessageReceivedEvent(IpcMessage message);

    public event IIpcChannel.MessageReceivedEvent? MessageReceived;

    #endregion

    #region Members

    private readonly NamedPipeServerStream _pipe;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _pipeLock = new(1, 1);
    private readonly ILogger<IpcChannel> _logger;

    #endregion

    #region Props

    public string Id { get; }

    #endregion

    #region Constructors

    public IpcChannel(string id, ILogger<IpcChannel> logger)
    {
        Id = id;
        _logger = logger;
        _pipe = new NamedPipeServerStream(
            Id,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            4096,
            4096
        );
        _reader = new StreamReader(_pipe);
        _writer = new StreamWriter(_pipe);
    }

    #endregion

    #region Public methods

    public void Dispose()
    {
        try
        {
            _pipeLock.Dispose();
            _pipe.Dispose();
            _reader.Dispose();
        } catch (Exception e)
        {
            _logger.LogError(e, "IPC Channel {Id}: Failed to dispose", Id);
        }
    }

    public Task SendMessageAsync(IpcMessage message)
    {
        return Write(message);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _pipe.WaitForConnectionAsync(cancellationToken);
            }
            catch (IOException e)
            {
                if (!e.Message.Contains("Pipe is broken")) throw;

                _logger.LogInformation("IPC Channel {Id}: Pipe closed or disconnected", Id);
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "IPC Channel {Id}: Failed to wait for connection", Id);
                await Task.Delay(1000, cancellationToken);
            }

            await ListenAsync(cancellationToken);
        }
    }

    #endregion

    #region Private methods

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (_pipe.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await Read();
                if (message is null) continue;

                MessageReceived?.Invoke(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "IPC Channel {Id}: Failed to read message", Id);
            }
        }
    }

    private async Task Write(IpcMessage message)
    {
        await _pipeLock.WaitAsync();

        try
        {
            await _writer.WriteLineAsync(message.ToString());
            await _writer.FlushAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "IPC Channel {Id}: Failed to write message", Id);
        }
        finally
        {
            _pipeLock.Release();
        }
    }

    private async Task<IpcMessage?> Read()
    {
        var data = await _reader.ReadLineAsync();
        if (string.IsNullOrEmpty(data) || data.Length == 2) return null;

        var bytes = Convert.FromBase64String(data[2..]);
        var json = Encoding.UTF8.GetString(bytes);
        try
        {
            return JsonSerializer.Deserialize<IpcMessage>(json);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "IPC Channel {Id}: Failed to deserialize message", Id);
        }

        return null;
    }

    #endregion
}