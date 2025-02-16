using CoverMe.Backend.Core.Enums.Ipc;
using CoverMe.Backend.Core.Extensions;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Extensions;

public class EnumExtensions
{
    #region Tests

    [InlineData(IpcMessageType.OpenFileAtLine, "openFileAtLine")]
    [InlineData(IpcMessageType.GetFileLineCoverage, "getFileLineCoverage")]
    [Theory]
    public void Description_ShouldTheStringValueOfEnum(IpcMessageType value, string expected)
    {
        // Arrange

        // Act
        var result = value.Description();

        // Assert
        result.ShouldBe(expected);
    }

    [InlineData("openFileAtLine", IpcMessageType.OpenFileAtLine)]
    [InlineData("getFileLineCoverage", IpcMessageType.GetFileLineCoverage)]
    [Theory]
    public void FromDescription_ShouldReturnTheEnumValue(string description, IpcMessageType expected)
    {
        // Arrange

        // Act
        var result = description.FromDescription<IpcMessageType>();

        // Assert
        result.ShouldBe(expected);
    }

    #endregion
}