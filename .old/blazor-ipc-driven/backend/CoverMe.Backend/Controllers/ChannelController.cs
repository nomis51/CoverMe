using CoverMe.Backend.Core.Ipc.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CoverMe.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChannelController : ControllerBase
{
    #region Members

    private readonly IIpcServer _ipcServer;

    #endregion

    #region Constructors

    public ChannelController(IIpcServer ipcServer)
    {
        _ipcServer = ipcServer;
    }

    #endregion

    #region Routes

    [HttpGet]
    public IActionResult GetChannelId()
    {
        var channelId = _ipcServer.CreateChannel();
        return string.IsNullOrEmpty(channelId) ? BadRequest() : Ok(channelId);
    }

    #endregion
}