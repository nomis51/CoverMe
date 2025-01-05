using CoverMe.Backend.Playwright.Tests.Extensions;
using FluentAssertions;

namespace CoverMe.Backend.Playwright.Tests.Components;

[TestFixture]
public class CoverageTableRowTests : PageTest
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
    public async Task CoverageTableRow_ShouldHavePaddingForAlignment()
    {
        // Arrange
        var row = Page.GetByTestId("coverage-table-row");
        var regPaddingLeft = new Regex("padding-left: [0-9]rem");

        // Act
        await foreach (var item in row.Enumerate())
        {
            // Assert
            var td = item.Locator("td").Nth(0);
            await Expect(td).ToHaveAttributeAsync("style", regPaddingLeft);
        }
    }

    [Test]
    public async Task CoverageTableRow_ShouldHaveIcon()
    {
        // Arrange
        var row = Page.GetByTestId("coverage-table-row");

        // Act
        await foreach (var item in row.Enumerate())
        {
            // Assert
            var img = item.Locator("td").Nth(0).Locator("img");
            await Expect(img).ToBeVisibleAsync();
        }
    }


    [Test]
    public async Task CoverageTableRow_ShouldHaveSymbolText()
    {
        // Arrange
        var row = Page.GetByTestId("coverage-table-row");

        // Act
        await foreach (var item in row.Enumerate())
        {
            // Assert
            var text = item.Locator("td").Nth(0).Locator(".mud-typography");
            await Expect(text).ToBeVisibleAsync();
            await Expect(text).Not.ToBeEmptyAsync();
        }
    }

    [Test]
    public async Task CoverageTableRow_ShouldHaveExpandButton_WhenExpandable()
    {
        // Arrange
        const int paddingLeftThatDoesntExpand = 4;
        var row = Page.GetByTestId("coverage-table-row");

        // Act
        await foreach (var item in row.Enumerate())
        {
            var td = item.Locator("td").Nth(0);
            var style = await td.GetAttributeAsync("style");
            style.Should().NotBeNullOrEmpty();

            var paddingLeft = int.Parse(new Regex("padding-left: ([0-9]+)rem").Match(style!).Groups[1].Value);
            if (paddingLeft <= paddingLeftThatDoesntExpand) continue;

            // Assert
            var expandButton = td.Locator("button");
            await Expect(expandButton).ToBeVisibleAsync();
            await Expect(expandButton).Not.ToBeDisabledAsync();
        }
    }

    [Test]
    public async Task CoverageTableRow_ShouldHavePercentBar()
    {
        // Arrange
        var row = Page.GetByTestId("coverage-table-row");
        var regPercent = new Regex("[0-9]+%");

        // Act
        await foreach (var item in row.Enumerate())
        {
            // Assert
            var bar = item.Locator("td").Nth(1);
            await Expect(bar).ToHaveTextAsync(regPercent);
        }
    }

    #endregion
}