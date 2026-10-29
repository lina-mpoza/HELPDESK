using Microsoft.AspNetCore.Mvc;
using HelpDesk.Data;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/db")]
public class DbController : ControllerBase
{
    private readonly DbHelper _db;
    public DbController(DbHelper db) => _db = db;

    // GET /api/db/ping
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var statusCount = await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Statuses");
        var personCount = await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Persons");

        return Ok(new
        {
            connected = true,
            database = "MunicipalHelpdesk",
            statusRows = Convert.ToInt32(statusCount),
            personRows = Convert.ToInt32(personCount),
            serverTime = DateTime.UtcNow
        });
    }
}