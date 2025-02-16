using System.Text;
using System.Text.Json;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Models.Ipc.Abstractions;

public class IpcMessageTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldCreateWithRandomId_WhenNothingIsProvided()
    {
        // Arrange
        // Act
        var sut = new IpcMessage();

        // Assert
        sut.Id.ShouldNotBeEmpty();
    }

    [Fact]
    public void Ctor_ShouldHaveNoData_WhenNothingIsProvided()
    {
        // Arrange
        // Act
        var sut = new IpcMessage();

        // Assert
        sut.HasData.ShouldBeFalse();
        sut.Data.ShouldBeNull();
    }

    [Fact]
    public void Ctor_ShouldSetId_WhenProvided()
    {
        // Arrange
        const string id = "SomeId";
        const string channelId = "SomeChannelId";
        const string type = "SomeType";
        const string data = "SomeData";

        // Act
        var sut = new IpcMessage(id, channelId, type, data);

        // Assert
        sut.Id.ShouldBe(id);
        sut.ChannelId.ShouldBe(channelId);
        sut.Type.ShouldBe(type);
        sut.Data.ShouldBe(data);
    }

    [Fact]
    public void Ctor_ShouldHaveSetTypeAndDataWithRandomId_WhenOnlyTypeAndDataAreProvided()
    {
        // Arrange
        const string channelId = "SomeChannelId";
        const string type = "SomeType";
        const string data = "SomeData";

        // Act
        var sut = new IpcMessage(channelId, type, data);

        // Assert
        sut.Id.ShouldNotBeEmpty();
        sut.ChannelId.ShouldBe(channelId);
        sut.Type.ShouldBe(type);
        sut.Data.ShouldBe(data);
    }

    [Fact]
    public void GetData_ShouldReturnTypedData_WhenTypeIsValid()
    {
        // Arrange
        var data = new List<string> { "SomeData" };
        var serializedData = JsonSerializer.Serialize(data);

        // Act
        var sut = new IpcMessage("channelId", "type", serializedData);

        // Assert
        sut.GetData<List<string>>().ShouldBeEquivalentTo(data);
    }

    [Fact]
    public void GetData_ShouldThrowException_WhenTypeIsInvalid()
    {
        // Arrange
        var data = new List<string> { "SomeData" };
        var serializedData = JsonSerializer.Serialize(data);
        var sut = new IpcMessage("channelId", "type", serializedData);

        // Act
        // Assert
        Assert.ThrowsAny<Exception>(() =>
            sut.GetData<Dictionary<string, int>>()
        );
    }

    [Fact]
    public void ToString_ShouldReturnBase64OfTheJson()
    {
        // Arrange
        const string id = "SomeId";
        const string channelId = "SomeChannelId";
        const string type = "SomeType";
        const string data = "SomeData";
        var expected = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{{\"id\":\"{id}\",\"channelId\":\"{channelId}\",\"type\":\"{type}\",\"data\":\"{data}\"}}"
            )
        );
        var sut = new IpcMessage(id, channelId, type, data);

        // Act
        var actual = sut.ToString();

        // Assert
        actual.ShouldBe(expected);
    }

    #endregion
}