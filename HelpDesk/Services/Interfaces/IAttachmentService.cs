using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface IAttachmentService
{
    Task<ApiResponse> UploadImageAsync(IFormFile file);

    /// <summary>Returns (bytes, fileName, mimeType) or null.</summary>
    Task<(byte[] bytes, string fileName, string mimeType)?> GetImageAsync(int imageId);
}