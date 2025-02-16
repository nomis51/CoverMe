namespace CoverMe.Backend.Core.Services.Abstractions;

public interface IIntellijService
{
    void OpenFileAtLine(string channelId, string filePath, int line);
    void SaveReport(string channelId, string reportFolder);
}