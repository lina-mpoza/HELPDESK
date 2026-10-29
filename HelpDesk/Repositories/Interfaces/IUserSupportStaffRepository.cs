using HelpDesk.DTOs.Responses;

namespace HelpDesk.Repositories.Interfaces;

public interface IUserSupportStaffRepository
{
    Task<List<TechnicianResponse>> GetAllAsync();
    Task<int?> GetUssIdByStaffIdAsync(int staffId);
    Task<int?> GetUssIdByPersonIdAsync(int personId);
    Task<int?> GetStaffIdByUssIdAsync(int ussId);
}