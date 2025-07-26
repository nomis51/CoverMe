using Microsoft.AspNetCore.Mvc;

namespace CoverMe.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IpcController : ControllerBase
{
    #region Routes

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("test");
    }

    #endregion
}