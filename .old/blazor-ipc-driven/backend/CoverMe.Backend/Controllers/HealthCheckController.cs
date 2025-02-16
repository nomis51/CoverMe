using Microsoft.AspNetCore.Mvc;

namespace CoverMe.Backend.Controllers;

[Route("api/health-check")]
[ApiController]
public class HealthCheckController : ControllerBase
{
    #region Routes

    [HttpGet]
    public IActionResult GetHealthCheck()
    {
        return Ok();
    }

    #endregion
}