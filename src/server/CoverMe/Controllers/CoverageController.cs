using System.Globalization;
using CoverMe.Core.Enums;
using CoverMe.Core.Models.Coverage;
using Microsoft.AspNetCore.Mvc;

namespace CoverMe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoverageController : ControllerBase
{
    #region Routes

    [HttpGet]
    public Task<IActionResult> Get()
    {
        var data = new[]
        {
            new CoverageItem
            {
                Type = CoverageItemType.Root,
                Visibility = CoverageItemVisibility.None,
                Symbol = "Total",
                Coverage = 78
            },
            new CoverageItem
            {
                Type = CoverageItemType.Project,
                Visibility = CoverageItemVisibility.None,
                Symbol = "Project A",
                Coverage = 78
            },
            new CoverageItem
            {
                Type = CoverageItemType.Project,
                Visibility = CoverageItemVisibility.None,
                Symbol = "Project A",
                Coverage = 67
            },
            new CoverageItem
            {
                Type = CoverageItemType.Namespace,
                Visibility = CoverageItemVisibility.None,
                Symbol = "Namespace A",
                Coverage = 78
            },
            new CoverageItem
            {
                Type = CoverageItemType.Class,
                Visibility = CoverageItemVisibility.Public,
                Symbol = "Class A",
                Coverage = 78
            },
            new CoverageItem
            {
                Type = CoverageItemType.Method,
                Visibility = CoverageItemVisibility.Public,
                Symbol = "Method A",
                Coverage = 78
            },
            new CoverageItem
            {
                Type = CoverageItemType.Property,
                Visibility = CoverageItemVisibility.Public,
                Symbol = "Property A",
                Coverage = 78
            },
            new CoverageItem
            {
                Type = CoverageItemType.Class,
                Visibility = CoverageItemVisibility.Public,
                Symbol = "Class B",
                Coverage = 67
            },
            new CoverageItem
            {
                Type = CoverageItemType.Method,
                Visibility = CoverageItemVisibility.Public,
                Symbol = "Method B",
                Coverage = 67
            },
            new CoverageItem
            {
                Type = CoverageItemType.Method,
                Visibility = CoverageItemVisibility.Private,
                Symbol = "Method B2",
                Coverage = 67
            },
            new CoverageItem
            {
                Type = CoverageItemType.Project,
                Visibility = CoverageItemVisibility.None,
                Symbol = "Project B",
                Coverage = 67
            },
            new CoverageItem
            {
                Type = CoverageItemType.Class,
                Visibility = CoverageItemVisibility.Public,
                Symbol = "Class C",
                Coverage = 67
            },
            new CoverageItem
            {
                Type = CoverageItemType.Method,
                Visibility = CoverageItemVisibility.Public,
                Symbol = "Method C",
                Coverage = 67
            }
        };
        return Task.FromResult<IActionResult>(Ok(data));
    }

    #endregion
}