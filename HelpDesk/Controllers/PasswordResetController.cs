using Microsoft.AspNetCore.Mvc;
using HelpDesk.DTOs.Requests;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/password")]
public class PasswordResetController : ControllerBase
{
    private readonly IPasswordResetService _reset;
    public PasswordResetController(IPasswordResetService reset) => _reset = reset;

    // POST /api/password/forgot
    [HttpPost("forgot")]
    public async Task<IActionResult> Forgot([FromBody] ForgotPasswordRequest req)
    {
        var res = await _reset.RequestOtpAsync(req);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // POST /api/password/reset
    [HttpPost("reset")]
    public async Task<IActionResult> Reset([FromBody] ResetPasswordRequest req)
    {
        var res = await _reset.ResetPasswordAsync(req);
        return res.Success ? Ok(res) : BadRequest(res);
    }
}