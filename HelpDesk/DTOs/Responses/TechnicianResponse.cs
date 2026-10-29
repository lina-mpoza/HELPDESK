namespace HelpDesk.DTOs.Responses;

public class TechnicianResponse
{
    public int UssId { get; set; }
    public int StaffId { get; set; }
    public string FullName { get; set; } = "";
    public string OfficeNo { get; set; } = "";
    public string OfficeLocation { get; set; } = "";
}