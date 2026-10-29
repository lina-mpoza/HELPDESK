using Microsoft.AspNetCore.Mvc;
using HelpDesk.DTOs.Requests;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    public AdminController(IAdminService admin) => _admin = admin;

    // GET /api/admin/tickets
    [HttpGet("tickets")]
    public async Task<IActionResult> GetTickets()
    {
        var res = await _admin.GetAllTicketsAsync();
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // GET /api/admin/technicians
    [HttpGet("technicians")]
    public async Task<IActionResult> GetTechnicians()
    {
        var res = await _admin.GetTechniciansAsync();
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/admin/assign/{personId}
    [HttpPost("assign/{personId:int}")]
    public async Task<IActionResult> Assign(int personId, [FromBody] AssignTicketRequest req)
    {
        var res = await _admin.AssignAsync(personId, req);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/admin/tickets/{ticketId}/close/{personId}
    [HttpPost("tickets/{ticketId:int}/close/{personId:int}")]
    public async Task<IActionResult> Close(int ticketId, int personId)
    {
        var res = await _admin.CloseTicketAsync(personId, ticketId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/admin/tickets/{ticketId}/reopen/{personId}
    [HttpPost("tickets/{ticketId:int}/reopen/{personId:int}")]
    public async Task<IActionResult> Reopen(int ticketId, int personId)
    {
        var res = await _admin.ReopenTicketAsync(personId, ticketId);
        return res.Success ? Ok(res) : BadRequest(res);
    }
}