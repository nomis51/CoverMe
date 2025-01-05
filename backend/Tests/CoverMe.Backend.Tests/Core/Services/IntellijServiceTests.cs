using System.Text.Json;
using CoverMe.Backend.Core.Enums.Ipc;
using CoverMe.Backend.Core.Extensions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using CoverMe.Backend.Core.Services;
using CoverMe.Backend.Core.Services.Abstractions;
using NSubstitute;

namespace CoverMe.Backend.Tests.Core.Services;

public class IntellijServiceTests
{
    #region Members

    private readonly IIpcManager _ipcManager;
    private readonly IIntellijService _sut;

    #endregion

    #region Constructors

    public IntellijServiceTests()
    {
        _ipcManager = Substitute.For<IIpcManager>();
        _sut = new IntellijService(_ipcManager);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task OpenFileAtLine_ShouldSendMessageToIpc()
    {
        // Arrange
        const string filePath = "path/to/file.cs";
        const int line = 42;
        var expectedData = JsonSerializer.Serialize(new
        {
            filePath = filePath.ConvertPathToUnix(),
            line
        });

        // Act
        _sut.OpenFileAtLine("1", filePath, line);

        // Assert
        _ipcManager
            .Received(1)
            .SendMessage(Arg.Is<IpcMessage>(e =>
                e.Type == IpcMessageType.OpenFileAtLine.Description() &&
                e.Data == expectedData
            ));
    }

    #endregion
}