namespace HelpDesk.DTOs.Requests;

public class UpdateUserByAdminRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string WorkEmail { get; set; } = "";
    public string CellNo { get; set; } = "";
}