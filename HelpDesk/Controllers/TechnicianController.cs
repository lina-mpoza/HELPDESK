using Microsoft.AspNetCore.Mvc;
using HelpDesk.DTOs.Requests;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/technician")]
public class TechnicianController : ControllerBase
{
    private readonly ITechnicianService _tech;
    public TechnicianController(ITechnicianService tech) => _tech = tech;

    // GET /api/technician/tickets/{personId}
    [HttpGet("tickets/{personId:int}")]
    public async Task<IActionResult> GetMine(int personId)
    {
        var res = await _tech.GetMyTicketsAsync(personId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/technician/tickets/{ticketId}/start/{personId}
    [HttpPost("tickets/{ticketId:int}/start/{personId:int}")]
    public async Task<IActionResult> Start(int ticketId, int personId)
    {
        var res = await _tech.StartTicketAsync(personId, ticketId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/technician/tickets/{ticketId}/resolve/{personId}
    [HttpPost("tickets/{ticketId:int}/resolve/{personId:int}")]
    public async Task<IActionResult> Resolve(int ticketId, int personId,
                                             [FromBody] ResolveTicketRequest req)
    {
        var res = await _tech.ResolveTicketAsync(personId, ticketId, req);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { ok = true, time = DateTime.UtcNow });
    }
}