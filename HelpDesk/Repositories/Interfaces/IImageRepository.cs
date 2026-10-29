using HelpDesk.Models;

namespace HelpDesk.Repositories.Interfaces;

public interface IImageRepository
{
    Task<int> CreateAsync(ImageRecord image);
    Task<ImageRecord?> GetByIdAsync(int imageId);
}

public class ImageRecord
{
    public int Image_ID { get; set; }
    public byte[]? Blob_Content { get; set; }
    public string? File_Name { get; set; }
    public string? Mime_Type { get; set; }
}