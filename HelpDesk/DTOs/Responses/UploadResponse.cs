namespace HelpDesk.DTOs.Responses;

public class UploadResponse
{
    public int ImageId { get; set; }
    public string FileName { get; set; } = "";
    public string MimeType { get; set; } = "";
    public long SizeBytes { get; set; }
}