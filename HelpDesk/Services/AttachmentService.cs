using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class AttachmentService : IAttachmentService
{
    // 5 MB max — tune to taste
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/gif",
        "image/webp"
    };

    private readonly IImageRepository _images;

    public AttachmentService(IImageRepository images)
    {
        _images = images;
    }

    public async Task<ApiResponse> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse.Fail("No file was provided.");

        if (file.Length > MaxFileSizeBytes)
            return ApiResponse.Fail($"File is too large (max {MaxFileSizeBytes / 1024 / 1024} MB).");

        var mime = (file.ContentType ?? "").ToLowerInvariant();
        if (!AllowedMimeTypes.Contains(mime))
            return ApiResponse.Fail("Only PNG, JPG, GIF, or WEBP images are allowed.");

        // Read the stream into memory
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        var imageId = await _images.CreateAsync(new ImageRecord
        {
            Blob_Content = bytes,
            File_Name = Path.GetFileName(file.FileName),
            Mime_Type = mime
        });

        return ApiResponse.Ok("Uploaded.", new UploadResponse
        {
            ImageId = imageId,
            FileName = Path.GetFileName(file.FileName),
            MimeType = mime,
            SizeBytes = file.Length
        });
    }

    public async Task<(byte[] bytes, string fileName, string mimeType)?> GetImageAsync(int imageId)
    {
        var img = await _images.GetByIdAsync(imageId);
        if (img == null || img.Blob_Content == null) return null;

        return (
            img.Blob_Content,
            img.File_Name ?? $"image-{imageId}",
            img.Mime_Type ?? "application/octet-stream"
        );
    }
}