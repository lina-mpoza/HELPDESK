namespace HelpDesk.DTOs.Responses;

public class AdminUserListItem
{
    public int    PersonId   { get; set; }
    public int?   StaffId    { get; set; }
    public string FullName   { get; set; } = "";
    public string Username   { get; set; } = "";
    public string WorkEmail  { get; set; } = "";
    public string CellNo     { get; set; } = "";
    public string StaffNo    { get; set; } = "";
    public string Role       { get; set; } = "";   // Admin | Technician | GeneralStaff | (No role)
    public string OfficeNo       { get; set; } = "";
    public string OfficeLocation { get; set; } = "";
    public DateTime? RegisteredAt { get; set; }
}