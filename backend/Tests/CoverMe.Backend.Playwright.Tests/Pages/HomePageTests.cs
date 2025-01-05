using CoverMe.Backend.Playwright.Tests.Extensions;
using Microsoft.Playwright;
using MudBlazor;

namespace CoverMe.Backend.Playwright.Tests.Pages;

[TestFixture]
public class HomePageTests : PageTest
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
    public async Task TestsProjectsSelect_ShouldAvailable()
    {
        // Arrange
        var expectedItems = new List<string>
        {
            "TestApp.Tests",
            "TestApp.Other.Tests"
        };
        var select = Page.GetByTestId("tests-projects-select");

        // Act
        await Expect(select).ToBeVisibleAsync();
        await Expect(select).Not.ToBeDisabledAsync();
        await select.AssertFieldLabel("Tests project");
        await select.AssertFieldPlaceholder("Select a tests project");
        await select.AssertDropdownItems(this, expectedItems);
    }

    [Test]
    public async Task FilterTestsTextfield_ShouldAvailable()
    {
        // Arrange
        var textfield = Page.GetByTestId("filter-tests-textfield");

        // Act
        await Expect(textfield).ToBeVisibleAsync();
        await Expect(textfield).Not.ToBeDisabledAsync();
        await textfield.AssertFieldLabel("Filter tests");
        await textfield.AssertFieldPlaceholder("Type something...");
    }

    [Test]
    [Retry(3)]
    public async Task FilterTestsTextField_ShouldBeClearable_WhenDirty()
    {
        // Arrange
        var textfield = Page.GetByTestId("filter-tests-textfield");
        var clearButton = textfield.GetParent()
            .Locator(".mud-input-clear-button");

        // Act
        await textfield.PressSequentiallyAsync("test", new LocatorPressSequentiallyOptions
        {
            Delay = 250
        });
        await Expect(clearButton).ToBeVisibleAsync();
        await clearButton.ClickAsync();
        await Expect(textfield).ToHaveValueAsync(string.Empty);
    }

    [TestCase("run-coverage-button", "Run coverage", Icons.Material.Outlined.PlayArrow)]
    [TestCase("build-and-run-coverage-button", "Build and run coverage", Icons.Material.Outlined.Build)]
    [TestCase("refresh-button", "Refresh", Icons.Material.Outlined.Refresh)]
    [Test]
    public async Task IconButtons_ShouldAvailable(
        string buttonTestId,
        string expectedTooltip,
        string expectedIcon
    )
    {
        // Arrange
        var button = Page.GetByTestId(buttonTestId);
        var icon = button.Locator("svg");

        // Act
        await Expect(button).ToBeVisibleAsync();
        await Expect(button).Not.ToBeDisabledAsync();
        await button.AssertTooltip(this, expectedTooltip);
        await icon.AssertIcon(expectedIcon);
    }

    [Test]
    public async Task SaveReportButton_ShouldAvailable()
    {
        // Arrange
        var menu = Page.GetByTestId("save-report-button");
        var button = menu.Locator("button");

        // Act
        await Expect(button).ToBeVisibleAsync();
        await Expect(button).Not.ToBeDisabledAsync();
        await button.AssertIcon(Icons.Material.Outlined.Save);
    }

    [Test]
    public async Task SaveReportMenu_ShouldShowReportTypeMenu_WhenClicked()
    {
        // Arrange
        var menu = Page.GetByTestId("save-report-button");
        var button = menu.Locator("button");

        // Act
        await button.AssertMenu(this, ["Simple report", "Detailed report"]);
    }

    [Test]
    public async Task CoverageTable_ShouldAvailable()
    {
        // Arrange
        var table = Page.GetByTestId("coverage-table");

        // Act
        await Expect(table).ToBeVisibleAsync();
    }

    #endregion
}