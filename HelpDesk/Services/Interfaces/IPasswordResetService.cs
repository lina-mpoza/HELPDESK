using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface IPasswordResetService
{
    Task<ApiResponse> RequestOtpAsync(ForgotPasswordRequest req);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest req);
}