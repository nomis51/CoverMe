using Shouldly;

namespace CoverMe.Backend.Playwright.Tests.Controllers;

[TestFixture]
public class ChannelControllerTests : PageTest
{
    #region Test

    [Test]
    public async Task GetChannelId_ShouldReturnChannelId()
    {
        // Arrange

        // Act
        var response = await Page.APIRequest.GetAsync($"{Constants.DevUrl}/api/channel");

        // Assert
        await Expect(response).ToBeOKAsync();

        var result = await response.TextAsync();
        result.ShouldNotBeNullOrEmpty();
    }

    #endregion
}