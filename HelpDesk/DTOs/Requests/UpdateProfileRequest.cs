namespace HelpDesk.DTOs.Requests;

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string WorkEmail { get; set; } = "";
    public string CellNo { get; set; } = "";

    /// <summary>Empty = keep current password.</summary>
    public string NewPassword { get; set; } = "";
}