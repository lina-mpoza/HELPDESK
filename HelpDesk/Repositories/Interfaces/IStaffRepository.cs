using HelpDesk.Models;

namespace HelpDesk.Repositories.Interfaces;

public interface IStaffRepository
{
    Task<int> CreateAsync(int personId, string? staffNo);
    Task<int?> GetStaffIdByPersonIdAsync(int personId);
    Task<string> GetRoleByStaffIdAsync(int staffId);
    Task<int?> GetMunicipalityIdByStaffIdAsync(int staffId);
}