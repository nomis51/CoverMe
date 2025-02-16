using System.ComponentModel;

namespace CoverMe.Backend.Core.Enums.Ipc;

public enum IpcMessageType
{
    [Description("openFileAtLine")]
    OpenFileAtLine = 0,

    [Description("getFileLineCoverage")]
    GetFileLineCoverage = 1,

    [Description("saveReport")]
    SaveReport = 2,
}