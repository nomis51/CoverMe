using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoverMe.Backend.Core.Enums.Ipc;
using CoverMe.Backend.Core.Extensions;

namespace CoverMe.Backend.Core.Models.Ipc.Abstractions;

public class IpcMessage
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("channelId")]
    public string ChannelId { get; init; } = null!;

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

    public IpcMessage(string id, string channelId, string type, string data)
    {
        Id = id;
        ChannelId = channelId;
        Type = type;
        Data = data;
    }

    public IpcMessage(string channelId, string type, string data)
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