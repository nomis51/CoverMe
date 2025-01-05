using System.Text.Json;
using CoverMe.Backend.Core.Enums.Ipc;
using CoverMe.Backend.Core.Extensions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using CoverMe.Backend.Core.Services.Abstractions;

namespace CoverMe.Backend.Core.Services;

public class IntellijService : IIntellijService
{
    #region Members

    private readonly IIpcManager _ipcManager;

    #endregion

    #region Constructors

    public IntellijService(IIpcManager ipcManager)
    {
        _ipcManager = ipcManager;
    }

    #endregion

    #region Public methods

    public void OpenFileAtLine(string channelId, string filePath, int line)
    {
        _ipcManager.SendMessage(new IpcMessage
        {
            ChannelId = channelId,
            Type = IpcMessageType.OpenFileAtLine.Description(),
            Data = JsonSerializer.Serialize(new
            {
                filePath = filePath.ConvertPathToUnix(),
                line
            }),
        });
    }

    public void SaveReport(string channelId, string reportFolder)
    {
        var unixPath = reportFolder.ConvertPathToUnix();

        _ipcManager.SendMessage(new IpcMessage
        {
            ChannelId = channelId,
            Type = IpcMessageType.SaveReport.Description(),
            Data = JsonSerializer.Serialize(new
            {
                reportFolder = unixPath,
            }),
        });
    }

    #endregion
}