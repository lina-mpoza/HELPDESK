namespace HelpDesk.Services.Interfaces;

public interface IEmailService
{
    Task SendTechnicianCredentialsAsync(
        string toEmail,
        string fullName,
        string username,
        string password,
        string staffNo,
        string officeLocation);

    // NEW
    Task SendPasswordResetOtpAsync(
        string toEmail,
        string fullName,
        string otp);
}