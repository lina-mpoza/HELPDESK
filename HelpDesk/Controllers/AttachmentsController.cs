using Microsoft.AspNetCore.Mvc;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Controllers;

[ApiController]
[Route("api/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachments;
    public AttachmentsController(IAttachmentService attachments)
        => _attachments = attachments;

    // POST /api/attachments/upload   (multipart/form-data, field = "file")
    [HttpPost("upload")]
    [RequestSizeLimit(6 * 1024 * 1024)]   // slightly above the 5 MB business cap
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var res = await _attachments.UploadImageAsync(file);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // GET /api/attachments/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await _attachments.GetImageAsync(id);
        if (result == null) return NotFound();

        var (bytes, fileName, mime) = result.Value;
        return File(bytes, mime, fileName);
    }
}