namespace HelpDesk.DTOs.Responses;

public class ProfileResponse
{
    public int PersonId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Username { get; set; } = "";
    public string WorkEmail { get; set; } = "";
    public string CellNo { get; set; } = "";
    public string StaffNo { get; set; } = "";
}