namespace HelpDesk.DTOs.Requests;

public class ResetPasswordRequest
{
    public string Email { get; set; } = "";
    public string Otp { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}