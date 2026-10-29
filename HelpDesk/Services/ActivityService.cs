using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class ActivityService : IActivityService
{
    private readonly ITicketActivityRepository _repo;
    public ActivityService(ITicketActivityRepository repo) => _repo = repo;

    public async Task<ApiResponse> GetAllAsync()
    {
        var list = await _repo.GetAllAsync(200);
        return ApiResponse.Ok($"{list.Count} activity entries.", list);
    }

    public async Task<ApiResponse> GetForTicketAsync(int ticketId)
    {
        var list = await _repo.GetForTicketAsync(ticketId);
        return ApiResponse.Ok($"{list.Count} entries for ticket #{ticketId}.", list);
    }
}