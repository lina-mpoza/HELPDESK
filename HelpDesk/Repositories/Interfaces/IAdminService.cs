using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface IAdminService
{
    Task<ApiResponse> GetAllTicketsAsync();
    Task<ApiResponse> GetTechniciansAsync();
    Task<ApiResponse> AssignAsync(int adminPersonId, AssignTicketRequest req);
    Task<ApiResponse> CloseTicketAsync(int adminPersonId, int ticketId);
    Task<ApiResponse> ReopenTicketAsync(int adminPersonId, int ticketId);
}