using CoverMe.Ipc.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CoverMe.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IpcController : ControllerBase
{
    #region Members

    private readonly IIpcService _ipcService;

    #endregion

    #region Constructors

    public IpcController(IIpcService ipcService)
    {
        _ipcService = ipcService;
    }

    #endregion

    #region Routes

    [HttpGet]
    public async Task<IActionResult> GetChannelId()
    {
        try
        {
            var channel = await _ipcService.CreateChannelAsync();
            return Ok(channel.Id);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    #endregion
}