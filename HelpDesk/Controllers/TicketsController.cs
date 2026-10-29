using HelpDesk.DTOs.Requests;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;
    private readonly IStaffRepository _staff;

    public TicketsController(ITicketService tickets, IStaffRepository staff)
    {
        _tickets = tickets;
        _staff = staff;
    }

    // GET /api/tickets/mine/{personId}
    [HttpGet("mine/{personId:int}")]
    public async Task<IActionResult> GetMine(int personId)
    {
        var staffId = await _staff.GetStaffIdByPersonIdAsync(personId);
        if (staffId == null)
            return NotFound(new { success = false, message = "No staff record for this user." });

        var result = await _tickets.GetMineAsync(staffId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST /api/tickets/{personId}
    [HttpPost("{personId:int}")]
    public async Task<IActionResult> Create(int personId, [FromBody] CreateTicketRequest req)
    {
        var staffId = await _staff.GetStaffIdByPersonIdAsync(personId);
        if (staffId == null)
            return NotFound(new { success = false, message = "No staff record for this user." });

        var result = await _tickets.CreateAsync(staffId.Value, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    // POST /api/tickets/{ticketId}/edit/{personId}
    [HttpPost("{ticketId:int}/edit/{personId:int}")]
    public async Task<IActionResult> Edit(int ticketId, int personId, [FromBody] EditTicketRequest req)
    {
        var res = await _tickets.EditAsync(personId, ticketId, req);
        return res.Success ? Ok(res) : BadRequest(res);
    }
    // POST /api/tickets/{ticketId}/cancel/{personId}
    [HttpPost("{ticketId:int}/cancel/{personId:int}")]
    public async Task<IActionResult> Cancel(int ticketId, int personId)
    {
        var res = await _tickets.CancelAsync(personId, ticketId);
        return res.Success ? Ok(res) : BadRequest(res);
    }
    // POST /api/tickets/{ticketId}/confirm/{personId}
    [HttpPost("{ticketId:int}/confirm/{personId:int}")]
    public async Task<IActionResult> Confirm(int ticketId, int personId)
    {
        var res = await _tickets.ConfirmAsync(personId, ticketId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/tickets/{ticketId}/reject-resolution/{personId}
    [HttpPost("{ticketId:int}/reject-resolution/{personId:int}")]
    public async Task<IActionResult> RejectResolution(int ticketId, int personId,
                                                       [FromBody] RejectResolutionRequest req)
    {
        var res = await _tickets.RejectResolutionAsync(personId, ticketId, req.Note);
        return res.Success ? Ok(res) : BadRequest(res);
    }
}