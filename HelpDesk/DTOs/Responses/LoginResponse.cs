namespace HelpDesk.DTOs.Responses;

public class LoginResponse
{
    public int PersonId { get; set; }
    public int StaffId { get; set; }
    public string Username { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Role { get; set; } = "";   // "Admin" | "Technician" | "GeneralStaff"
}