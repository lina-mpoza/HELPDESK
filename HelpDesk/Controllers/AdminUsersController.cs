using Microsoft.AspNetCore.Mvc;
using HelpDesk.DTOs.Requests;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUsersService _users;
    public AdminUsersController(IAdminUsersService users) => _users = users;

    // GET /api/admin/users/lookups/{adminPersonId}
    [HttpGet("lookups/{adminPersonId:int}")]
    public async Task<IActionResult> GetLookups(int adminPersonId)
    {
        var res = await _users.GetLookupsAsync(adminPersonId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // GET /api/admin/users/departments/{municipalityId}
    [HttpGet("departments/{municipalityId:int}")]
    public async Task<IActionResult> GetDepartments(int municipalityId)
    {
        var res = await _users.GetDepartmentsForMunicipalityAsync(municipalityId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/admin/users/technician
    [HttpPost("technician")]
    public async Task<IActionResult> CreateTechnician([FromBody] CreateTechnicianRequest req)
    {
        var res = await _users.CreateTechnicianAsync(req);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // GET /api/admin/users/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var res = await _users.GetAllUsersAsync();
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // GET /api/admin/users/profile/{personId}
    [HttpGet("profile/{personId:int}")]
    public async Task<IActionResult> GetProfile(int personId)
    {
        var res = await _users.GetProfileAsync(personId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/admin/users/profile/{personId}
    [HttpPost("profile/{personId:int}")]
    public async Task<IActionResult> UpdateProfile(int personId, [FromBody] UpdateProfileRequest req)
    {
        var res = await _users.UpdateProfileAsync(personId, req);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/admin/users/{personId}/edit
    [HttpPost("{personId:int}/edit")]
    public async Task<IActionResult> UpdateUser(int personId, [FromBody] UpdateUserByAdminRequest req)
    {
        var res = await _users.UpdateUserByAdminAsync(personId, req);
        return res.Success ? Ok(res) : BadRequest(res);
    }
}