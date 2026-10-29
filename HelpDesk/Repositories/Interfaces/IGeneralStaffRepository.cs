namespace HelpDesk.Repositories.Interfaces;

public interface IGeneralStaffRepository
{
    Task<int> CreateAsync(int staffId);
}