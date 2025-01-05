using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace CoverMe.Backend.Playwright.Tests.Extensions;

public static class LocatorExtensions
{
    #region Public methods

    public static async IAsyncEnumerable<ILocator> Enumerate(this ILocator locator)
    {
        var count = await locator.CountAsync();
        for (var i = 0; i < count; i++)
        {
            yield return locator.Nth(i);
        }
    }

    public static async Task AssertMenu(this ILocator element, PageTest pageTest, IEnumerable<string> items)
    {
        var popover = await GetPopover(pageTest, element, 1);
        Assert.That(popover, Is.Not.Null);

        var listItems = popover.Locator(".mud-list-item");
        var html = await popover.InnerHTMLAsync();
        var nbListItems = await listItems.CountAsync();
        var lstItems = items.ToList();
        nbListItems.Should().Be(lstItems.Count);

        for (var i = 0; i < nbListItems; i++)
        {
            var listItem = listItems.Nth(i);
            var listItemText = await listItem.InnerTextAsync();
            Assert.That(listItemText, Is.Not.Null);

            listItemText.Should().Be(lstItems[i]);
        }
    }

    public static async Task AssertIcon(this ILocator element, string expectedIcon)
    {
        var svgStr = await element.InnerHTMLAsync();
        var svg = new HtmlDocument();
        svg.LoadHtml(svgStr);

        var expectedIconHtml = new HtmlDocument();
        expectedIconHtml.LoadHtml(expectedIcon);

        svg.DocumentNode.OuterHtml.Should().ContainEquivalentOf(expectedIconHtml.DocumentNode.OuterHtml);
    }

    public static async Task AssertTooltip(this ILocator element, PageTest pageTest, string text, int hoverDelay = 100)
    {
        Assert.That(element, Is.Not.Null);
        var count = await element.CountAsync();
        count.Should().Be(1);

        var tooltip = element.GetParent();
        Assert.That(tooltip, Is.Not.Null);
        await pageTest.Expect(tooltip).ToHaveClassAsync(new Regex("mud-tooltip-root"));

        var popoverValue = tooltip.Locator(".mud-popover-cascading-value");
        Assert.That(popoverValue, Is.Not.Null);

        var popoverId = await popoverValue.Nth(0).GetAttributeAsync("id");
        Assert.That(popoverId, Is.Not.Null);
        popoverId.Should().NotBeEmpty();
        popoverId = popoverId.Replace("popover", "popovercontent");

        var popover = pageTest.Page.Locator($"#{popoverId}");
        Assert.That(popover, Is.Not.Null);

        await tooltip.HoverAsync();
        await Task.Delay(hoverDelay);

        var tooltipText = await popover.InnerTextAsync();
        Assert.That(tooltipText, Is.Not.Null);
        tooltipText.Should().Be(text);

        await popover.BlurAsync();
    }

    public static async Task AssertFieldLabel(
        this ILocator element,
        string text,
        int parentDepth = 2
    )
    {
        Assert.That(element, Is.Not.Null);
        var container = GetParent(element, "div", parentDepth);
        Assert.That(container, Is.Not.Null);

        var label = container.Locator(".mud-input-label");
        Assert.That(label, Is.Not.Null);

        var labelText = await label.InnerTextAsync();
        Assert.That(labelText, Is.Not.Null);
        labelText.Should().Be(text);
    }

    public static async Task AssertFieldPlaceholder(this ILocator element, string text)
    {
        Assert.That(element, Is.Not.Null);
        var placeholder = await element.GetAttributeAsync("placeholder");
        placeholder.Should().Be(text);
    }

    public static async Task AssertAnyDropdownItems(this ILocator element, PageTest pageTest)
    {
        Assert.That(element, Is.Not.Null);
        var popover = await GetPopover(pageTest, element);
        Assert.That(popover, Is.Not.Null);

        var listItems = popover.Locator(".mud-list-item");
        var nbListItems = await listItems.CountAsync();
        nbListItems.Should().BeGreaterThan(0);
    }

    public static async Task AssertDropdownItems(this ILocator element, PageTest pageTest, IEnumerable<string> items)
    {
        Assert.That(element, Is.Not.Null);
        var popover = await GetPopover(pageTest, element);
        Assert.That(popover, Is.Not.Null);

        var listItems = popover.Locator(".mud-list-item");
        var nbListItems = await listItems.CountAsync();
        var lstItems = items.ToList();
        nbListItems.Should().Be(lstItems.Count);

        for (var i = 0; i < nbListItems; i++)
        {
            var listItem = listItems.Nth(i);
            var listItemText = await listItem.InnerTextAsync();
            Assert.That(listItemText, Is.Not.Null);

            listItemText.Should().Be(lstItems[i]);
        }
    }

    public static async Task SelectDropdownItem(
        this ILocator element,
        PageTest pageTest,
        string text,
        bool forceCloseAfterSelection = false,
        int forceCloseOffset = 5
    )
    {
        Assert.That(element, Is.Not.Null);
        var popover = await GetPopover(pageTest, element);
        Assert.That(popover, Is.Not.Null);

        var listItems = popover.Locator(".mud-list-item");
        var nbListItems = await listItems.CountAsync();

        for (var i = 0; i < nbListItems; i++)
        {
            var listItem = listItems.Nth(i);
            var listItemText = await listItem.InnerTextAsync();
            Assert.That(listItemText, Is.Not.Null);
            if (listItemText != text) continue;

            // some items can't be clicked the "official" way if they are not visible or intersecting
            await listItem.DispatchEventAsync("click");

            if (forceCloseAfterSelection)
            {
                await ClosePopover(pageTest, popover, forceCloseOffset);
            }

            return;
        }

        Assert.That(false, Is.True, $"Unable to select dropdown item '{text}'");
    }

    public static async Task SelectDropdownItem(
        this ILocator element,
        PageTest pageTest,
        int index,
        bool forceCloseAfterSelection = false,
        int forceCloseOffset = 5
    )
    {
        Assert.That(element, Is.Not.Null);
        var popover = await GetPopover(pageTest, element);
        Assert.That(popover, Is.Not.Null);

        var listItems = popover.Locator(".mud-list-item");
        var nbListItems = await listItems.CountAsync();
        nbListItems.Should().BeGreaterThan(index);

        var listItem = listItems.Nth(index);

        await listItem.ClickAsync();

        if (forceCloseAfterSelection)
        {
            await ClosePopover(pageTest, popover, forceCloseOffset);
        }
    }

    public static ILocator GetParent(this ILocator element)
    {
        Assert.That(element, Is.Not.Null);
        return element.Locator("xpath=..");
    }

    public static ILocator GetParent(this ILocator element, string elementName, int depth = 1)
    {
        Assert.That(element, Is.Not.Null);
        return element.Locator($"xpath=/ancestor::{elementName}[{depth}]");
    }

    public static async Task<ILocator?> GetPopover(PageTest pageTest, ILocator element, int parentDepth = 2)
    {
        Assert.That(element, Is.Not.Null);
        var container = GetParent(element, "div", parentDepth);
        Assert.That(container, Is.Not.Null);

        if (await element.IsHiddenAsync())
        {
            // if the element is hidden, playwrigtht can't click on it
            await element.DispatchEventAsync("click");
        }
        else
        {
            await element.ClickAsync();
        }

        var popoverValue = container.Locator("xpath=/div[@class='mud-popover-cascading-value']");
        var popoverId = await popoverValue.GetAttributeAsync("id");
        Assert.That(popoverId, Is.Not.Null);
        popoverId = popoverId.Replace("popover", "popovercontent");
        popoverId.Should().NotBeEmpty();

        var popover = pageTest.Page.Locator($"#{popoverId}");
        Assert.That(popover, Is.Not.Null);
        await pageTest.Expect(popover).ToBeVisibleAsync();

        return popover;
    }

    public static async Task ClosePopover(PageTest pageTest, ILocator popover, int closeOffset = 5)
    {
        if (await popover.IsVisibleAsync())
        {
            var box = await popover.BoundingBoxAsync();
            Assert.That(box, Is.Not.Null);

            await pageTest.Page.Mouse.ClickAsync(box.X + box.Width + closeOffset, box.Y);
            var now = DateTime.Now;

            while (await popover.IsVisibleAsync() && (DateTime.Now - now).TotalMilliseconds < 500)
            {
                await Task.Delay(100);
            }

            if (await popover.IsVisibleAsync())
            {
                await pageTest.Page.Mouse.ClickAsync(box.X - closeOffset, box.Y);
            }

            await pageTest.Expect(popover).ToBeHiddenAsync();
        }
    }

    #endregion
}