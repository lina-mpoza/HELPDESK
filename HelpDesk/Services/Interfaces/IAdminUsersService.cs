using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface IAdminUsersService
{
    Task<ApiResponse> GetLookupsAsync(int adminPersonId);
    Task<ApiResponse> GetDepartmentsForMunicipalityAsync(int municipalityId);
    Task<ApiResponse> CreateTechnicianAsync(CreateTechnicianRequest req);
    Task<ApiResponse> GetAllUsersAsync();
    Task<ApiResponse> GetProfileAsync(int personId);
    Task<ApiResponse> UpdateProfileAsync(int personId, UpdateProfileRequest req);
    Task<ApiResponse> UpdateUserByAdminAsync(int personId, UpdateUserByAdminRequest req);
}