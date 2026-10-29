using HelpDesk.DTOs.Responses;

namespace HelpDesk.Repositories.Interfaces;

public interface ITicketActivityRepository
{
    /// Insert one activity row
    Task LogAsync(
        int ticketId,
        int? performedByStaffId,
        string action,
        string? oldValue,
        string? newValue,
        string? note);

    /// Recent activity across all tickets.
    Task<List<ActivityResponse>> GetAllAsync(int limit = 200);

    /// Timeline for a single ticket.
    Task<List<ActivityResponse>> GetForTicketAsync(int ticketId);
}