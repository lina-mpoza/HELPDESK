using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse> RegisterAsync(RegisterRequest req);
    Task<ApiResponse> LoginAsync(LoginRequest req);
}