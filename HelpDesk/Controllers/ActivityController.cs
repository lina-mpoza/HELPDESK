using Microsoft.AspNetCore.Mvc;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activity;
    public ActivityController(IActivityService activity) => _activity = activity;

    // GET /api/activity
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var res = await _activity.GetAllAsync();
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // GET /api/activity/ticket/{ticketId}
    [HttpGet("ticket/{ticketId:int}")]
    public async Task<IActionResult> GetForTicket(int ticketId)
    {
        var res = await _activity.GetForTicketAsync(ticketId);
        return res.Success ? Ok(res) : BadRequest(res);
    }
}