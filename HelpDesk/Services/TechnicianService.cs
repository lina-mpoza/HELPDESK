using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class TechnicianService : ITechnicianService
{
    private readonly ITicketRepository _tickets;
    private readonly IUserSupportStaffRepository _uss;
    private readonly IUSSTicketTaskRepository _tasks;
    private readonly ITicketActivityRepository _activity;

    public TechnicianService(
        ITicketRepository tickets,
        IUserSupportStaffRepository uss,
        IUSSTicketTaskRepository tasks,
        ITicketActivityRepository activity)
    {
        _tickets = tickets;
        _uss = uss;
        _tasks = tasks;
        _activity = activity;
    }

    // GET MY TICKETS
    public async Task<ApiResponse> GetMyTicketsAsync(int personId)
    {
        var ussId = await _uss.GetUssIdByPersonIdAsync(personId);

        if (ussId == null)
            return ApiResponse.Fail(
                "You are not registered as user support staff.");

        var rows = await _tickets.GetByUssIdAsync(ussId.Value);

        var result = rows.Select(r => new TechnicianTicketResponse
        {
            TicketId = r.Ticket_ID,
            CreatedDate = r.Created_Date,
            IssueDescription = r.Issue_Description ?? "",
            UrgencyLevel = r.Urgency_Level ?? "",
            CurrentStatusId = r.Current_Status_ID,
            CurrentStatus = r.Current_Status ?? "Unknown",
            SubmittedBy = r.SubmittedByName ?? "(unknown)",
            AssignDate = r.Assign_Date,
            Deadline = r.Deadline,
            AssignmentNote = r.AssignmentInstruction ?? "",
            ResolvedAt = r.Resolved_At,
            ResolutionNote = r.ResolutionNote ?? "",
            ImageId = r.Image_ID,
        }).ToList();

        return ApiResponse.Ok(
            $"{result.Count} ticket(s).",
            result);
    }

    // START TICKET
    public async Task<ApiResponse> StartTicketAsync(
        int personId,
        int ticketId)
    {
        var ussId = await _uss.GetUssIdByPersonIdAsync(personId);

        if (ussId == null)
            return ApiResponse.Fail(
                "You are not registered as user support staff.");

        var statusId =
            await _tickets.GetStatusIdByNameAsync("InProgress");

        if (statusId == null)
            return ApiResponse.Fail(
                "Status 'InProgress' is not configured.");

        // Update ticket status
        await _tickets.UpdateStatusAsync(
            ticketId,
            statusId.Value,
            null,
            null);

        // Log to USSTicketsTasks
        await _tasks.CreateAsync(
            ticketId,
            ussId.Value,
            statusId.Value,
            "Work started",
            null);

        // Get technician's Staff_ID
        var staffId =
            await _uss.GetStaffIdByUssIdAsync(ussId.Value);

        // Log activity
        await _activity.LogAsync(
            ticketId,
            staffId,
            "Started",
            "Assigned",
            "InProgress",
            "Technician started working");

        return ApiResponse.Ok("Ticket started.");
    }

    // RESOLVE TICKET (InProgress → Resolved)
    public async Task<ApiResponse> ResolveTicketAsync(
        int personId,
        int ticketId,
        ResolveTicketRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Note))
            return ApiResponse.Fail(
                "Please provide a resolution note.");

        if (req.Note.Length > 100)
            return ApiResponse.Fail(
                "Resolution note must be 100 characters or less.");

        var ussId = await _uss.GetUssIdByPersonIdAsync(personId);

        if (ussId == null)
            return ApiResponse.Fail(
                "You are not registered as user support staff.");

        var statusId =
            await _tickets.GetStatusIdByNameAsync("Resolved");

        if (statusId == null)
            return ApiResponse.Fail(
                "Status 'Resolved' is not configured.");

        var now = DateTime.UtcNow;

        // Update ticket status + Resolved_At
        await _tickets.UpdateStatusAsync(
            ticketId,
            statusId.Value,
            now,
            null);

        // Log to USSTicketsTasks
        await _tasks.CreateAsync(
            ticketId,
            ussId.Value,
            statusId.Value,
            req.Note.Trim(),
            now);

        // Get technician's Staff_ID
        var staffId =
            await _uss.GetStaffIdByUssIdAsync(ussId.Value);

        // Log activity
        await _activity.LogAsync(
            ticketId,
            staffId,
            "Resolved",
            "InProgress",
            "Resolved",
            req.Note.Trim());

        return ApiResponse.Ok("Ticket resolved.");
    }
}