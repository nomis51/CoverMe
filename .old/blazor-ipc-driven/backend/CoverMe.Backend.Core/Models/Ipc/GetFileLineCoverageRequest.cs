using System.Text.Json.Serialization;

namespace CoverMe.Backend.Core.Models.Ipc;

public class GetFileLineCoverageRequest
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("projectRootPath")]
    public string ProjectRootPath { get; set; } = string.Empty;
}