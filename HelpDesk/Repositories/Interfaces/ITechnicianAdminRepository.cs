using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;

namespace HelpDesk.Repositories.Interfaces;

public interface ITechnicianAdminRepository
{
    Task<(bool ok, string message, int personId)> CreateTechnicianAsync(CreateTechnicianRequest req);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> StaffNoExistsAsync(string staffNo);
    Task<List<AdminUserListItem>> GetAllUsersAsync();
    Task<ProfileResponse?> GetProfileAsync(int personId);
    Task<bool> UpdateProfileAsync(int personId, UpdateProfileRequest req);
    Task<bool> UpdateUserByAdminAsync(int personId, UpdateUserByAdminRequest req);
    Task<bool> WorkEmailExistsAsync(string email);
    Task<bool> CellNoExistsAsync(string cellNo);
}