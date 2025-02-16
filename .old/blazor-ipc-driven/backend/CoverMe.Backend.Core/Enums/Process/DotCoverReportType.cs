using System.ComponentModel;

namespace CoverMe.Backend.Core.Enums.Process;

public enum DotCoverReportType
{
    [Description("DetailedXML")]
    DetailedXml,

    [Description("HTML")]
    Html,
}