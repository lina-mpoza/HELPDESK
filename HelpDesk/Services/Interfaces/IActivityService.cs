using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface IActivityService
{
    Task<ApiResponse> GetAllAsync();
    Task<ApiResponse> GetForTicketAsync(int ticketId);
}