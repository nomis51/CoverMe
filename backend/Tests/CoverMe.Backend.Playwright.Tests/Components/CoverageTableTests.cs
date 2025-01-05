using FluentAssertions;
using Microsoft.Playwright;

namespace CoverMe.Backend.Playwright.Tests.Components;

[TestFixture]
public class CoverageTableTests : PageTest
{
    #region Setup

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync(Constants.DevUrl);
        await Expect(Page.GetByTestId("home-page")).ToBeVisibleAsync();
        await Expect(Page).ToHaveTitleAsync("Home");
    }

    #endregion

    #region Tests

    [Test]
    public async Task CoverageTable_ShouldHaveCoverageTableRowItems()
    {
        // Arrange
        var table = Page.GetByTestId("coverage-table");
        var items = table.GetByTestId("coverage-table-row");

        // Act
        var count = await items.CountAsync();
        count.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task CoverageTable_ShouldHaveThoseHeaders()
    {
        // Arrange
        var headers = new[] { "Symbol", "Coverage" };

        // Act
        var table = Page.GetByTestId("coverage-table");
        var actualHeaders = await table.GetByRole(AriaRole.Columnheader).AllTextContentsAsync();

        // Assert
        actualHeaders.Should().BeEquivalentTo(headers);
    }

    #endregion
}