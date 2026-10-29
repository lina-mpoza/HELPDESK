using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface ITechnicianService
{
    Task<ApiResponse> GetMyTicketsAsync(int personId);
    Task<ApiResponse> StartTicketAsync(int personId, int ticketId);
    Task<ApiResponse> ResolveTicketAsync(int personId, int ticketId, ResolveTicketRequest req);
}