using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoverMe.Ipc.Enums;
using CoverMe.Shared.Extensions;

namespace CoverMe.Ipc.Models.Abstractions;

public class IpcMessage
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonPropertyName("channelId")]
    public Guid ChannelId { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = null!;

    [JsonPropertyName("data")]
    public string? Data { get; init; }

    [JsonIgnore]
    public bool HasData => Data is not null && !string.IsNullOrWhiteSpace(Data);

    [JsonIgnore]
    public IpcMessageType MessageType => Type.FromDescription<IpcMessageType>();

    public IpcMessage()
    {
    }

    public IpcMessage(Guid id, Guid channelId, string type, string data)
    {
        Id = id;
        ChannelId = channelId;
        Type = type;
        Data = data;
    }

    public IpcMessage(Guid channelId, string type, string data)
    {
        ChannelId = channelId;
        Type = type;
        Data = data;
    }

    public T GetData<T>()
    {
        return JsonSerializer.Deserialize<T>(Data!)!;
    }

    public override string ToString()
    {
        return Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(this)
            )
        );
    }
}