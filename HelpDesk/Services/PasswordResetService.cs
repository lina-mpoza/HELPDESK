using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IPersonRepository _persons;
    private readonly IEmailService _email;

    public PasswordResetService(IPersonRepository persons, IEmailService email)
    {
        _persons = persons;
        _email = email;
    }

    public async Task<ApiResponse> RequestOtpAsync(ForgotPasswordRequest req)
    {
        var email = req.Email?.Trim();

        if (string.IsNullOrWhiteSpace(email))
            return ApiResponse.Fail("Please enter your email address.");

        // Deliberately do not reveal whether the email exists
        var person = await _persons.GetByEmailAsync(email);
        if (person == null)
        {
            // Pretend it worked so no enumeration is possible
            return ApiResponse.Ok(
                "If your email is registered, an OTP has been sent.");
        }

        // Generate a 6-digit code
        var rng = new Random();
        var otp = rng.Next(100000, 999999).ToString();

        // Expire in 10 minutes (UTC)
        var expiry = DateTime.UtcNow.AddMinutes(10);
        await _persons.SaveResetOtpAsync(person.Person_ID, otp, expiry);

        // Send the email
        try
        {
            var fullName = $"{person.First_Name} {person.Last_Name}".Trim();
            await _email.SendPasswordResetOtpAsync(email, fullName, otp);

            return ApiResponse.Ok(
                "If your email is registered, an OTP has been sent.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OTP email failed: {ex}");
            return ApiResponse.Fail("Could not send the OTP email. Please try again later.");
        }
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest req)
    {
        var email = req.Email?.Trim();
        var otp = req.Otp?.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(otp) ||
            string.IsNullOrWhiteSpace(req.NewPassword))
            return ApiResponse.Fail("All fields are required.");

        if (req.NewPassword.Length < 6)
            return ApiResponse.Fail("Password must be at least 6 characters.");

        if (req.NewPassword != req.ConfirmPassword)
            return ApiResponse.Fail("Passwords do not match.");

        var (ok, message) = await _persons.VerifyOtpAndResetPasswordAsync(
            email, otp, req.NewPassword);

        return ok ? ApiResponse.Ok(message) : ApiResponse.Fail(message);
    }
}