using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;

namespace HelpDesk.Services.Interfaces;

public interface ITicketService
{
    Task<ApiResponse> CreateAsync(int staffId, CreateTicketRequest req);
    Task<ApiResponse> GetMineAsync(int staffId);
    Task<ApiResponse> EditAsync(int personId, int ticketId, EditTicketRequest req);
    Task<ApiResponse> CancelAsync(int personId, int ticketId);
    Task<ApiResponse> ConfirmAsync(int personId, int ticketId);
    Task<ApiResponse> RejectResolutionAsync(int personId, int ticketId, string note);
}