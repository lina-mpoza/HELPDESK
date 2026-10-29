namespace HelpDesk.Repositories.Interfaces;

public interface IItAdminRepository
{
    Task<int?> GetAdminIdByStaffIdAsync(int staffId);
    Task<int?> GetAdminIdByPersonIdAsync(int personId);
}