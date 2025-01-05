using System.Text;
using System.Text.Json;
using CoverMe.Backend.Core.Models.Ipc.Abstractions;
using FluentAssertions;

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
        sut.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Ctor_ShouldHaveNoData_WhenNothingIsProvided()
    {
        // Arrange
        // Act
        var sut = new IpcMessage();

        // Assert
        sut.HasData.Should().BeFalse();
        sut.Data.Should().BeNull();
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
        sut.Id.Should().Be(id);
        sut.ChannelId.Should().Be(channelId);
        sut.Type.Should().Be(type);
        sut.Data.Should().Be(data);
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
        sut.Id.Should().NotBeEmpty();
        sut.ChannelId.Should().Be(channelId);
        sut.Type.Should().Be(type);
        sut.Data.Should().Be(data);
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
        sut.GetData<List<string>>().Should().BeEquivalentTo(data);
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
        actual.Should().Be(expected);
    }

    #endregion
}