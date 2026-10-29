using HelpDesk.DTOs.Responses;

namespace HelpDesk.Repositories.Interfaces;

public interface ILookupRepository
{
    Task<List<LookupItem>> GetJobTitlesAsync();
    Task<List<LookupItem>> GetEmploymentStatusesAsync();

    /// Returns the single municipality the admin belongs to
    Task<LookupItem?> GetAdminMunicipalityAsync(int adminPersonId);

    /// Returns departments that exist in the given municipality
    Task<List<LookupItem>> GetDepartmentsForMunicipalityAsync(int municipalityId);
}