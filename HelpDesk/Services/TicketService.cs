using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;
using HelpDesk.Models;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _tickets;
    private readonly IStaffRepository _staff;
    private readonly ITicketActivityRepository _activity;

    public TicketService(
        ITicketRepository tickets,
        IStaffRepository staff,
        ITicketActivityRepository activity)
    {
        _tickets = tickets;
        _staff = staff;
        _activity = activity;
    }

   public async Task<ApiResponse> CreateAsync(int staffId, CreateTicketRequest req)
{
    if (string.IsNullOrWhiteSpace(req.IssueDescription))
        return ApiResponse.Fail("Please describe the issue.");

    if (req.IssueDescription.Length > 200)
        return ApiResponse.Fail("Description must be 200 characters or less.");

    var statusId = await _tickets.GetStatusIdByNameAsync("Open");
    if (statusId == null)
        return ApiResponse.Fail("Status 'Open' is not configured in the database.");

    var ticketId = await _tickets.CreateAsync(new IssueTicket
    {
        Tech_Issue_ID = req.TechIssueId,
        Urgency_Level = string.IsNullOrWhiteSpace(req.UrgencyLevel)
            ? "Medium"
            : req.UrgencyLevel,
        Image_ID = req.ImageId,
        Video_ID = req.VideoId,
        Issue_Description = req.IssueDescription.Trim(),
        Staff_ID = staffId,
        Current_Status_ID = statusId
    });

    // Log ticket creation activity
    await _activity.LogAsync(
        ticketId,
        staffId,                 // The staff member who submitted the ticket
        "Created",
        null,                    // Previous status
        "Open",                  // New status
        "Ticket submitted"
    );

    return ApiResponse.Ok(
        "Ticket submitted successfully.",
        new { ticketId }
    );
}

    public async Task<ApiResponse> GetMineAsync(int staffId)
    {
        var tickets = await _tickets.GetByStaffIdAsync(staffId);

        var result = new List<TicketResponse>();
        foreach (var t in tickets)
        {
            var statusName = t.Current_Status_ID != null
                ? await _tickets.GetStatusNameAsync(t.Current_Status_ID.Value) ?? "Unknown"
                : "Unknown";

            result.Add(new TicketResponse
            {
                TicketId = t.Ticket_ID,
                CreatedDate = t.Created_Date,
                IssueDescription = t.Issue_Description ?? "",
                UrgencyLevel = t.Urgency_Level ?? "",
                CurrentStatusId = t.Current_Status_ID,
                CurrentStatus = statusName,
                ResolvedAt = t.Resolved_At,
                ClosedAt = t.Closed_At,
                ImageId = t.Image_ID  
            });
        }

        return ApiResponse.Ok($"{result.Count} ticket(s) found.", result);
    }
    public async Task<ApiResponse> EditAsync(int personId, int ticketId, EditTicketRequest req)
    {
        // ---- Validation ----
        if (string.IsNullOrWhiteSpace(req.IssueDescription))
            return ApiResponse.Fail("Please describe the issue.");

        if (req.IssueDescription.Length > 200)
            return ApiResponse.Fail("Description must be 200 characters or less.");

        // ---- Resolve staff ----
        var staffId = await _staff.GetStaffIdByPersonIdAsync(personId);
        if (staffId == null)
            return ApiResponse.Fail("No staff record linked to this account.");

        // ---- Load ticket ----
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ApiResponse.Fail("Ticket not found.");

        // ---- Must be the owner ----
        if (ticket.Staff_ID != staffId.Value)
            return ApiResponse.Fail("You can only edit tickets you created.");

        // ---- Must be Open ----
        var openStatusId = await _tickets.GetStatusIdByNameAsync("Open");
        if (openStatusId == null)
            return ApiResponse.Fail("Status 'Open' is not configured.");

        if (ticket.Current_Status_ID != openStatusId.Value)
            return ApiResponse.Fail("You can only edit tickets that are still Open.");

        // ---- Apply change ----
        var ok = await _tickets.UpdateDescriptionAndUrgencyAsync(
            ticketId,
            req.IssueDescription.Trim(),
            string.IsNullOrWhiteSpace(req.UrgencyLevel) ? "Medium" : req.UrgencyLevel);

        if (!ok)
            return ApiResponse.Fail("Could not update ticket.");

        // ---- Audit ----
        await _activity.LogAsync(
            ticketId,
            staffId.Value,
            "Edited",
            ticket.Issue_Description,     // old value
            req.IssueDescription.Trim(),  // new value
            "Description and/or urgency updated by reporter");

        return ApiResponse.Ok("Ticket updated successfully.");
    }

    public async Task<ApiResponse> CancelAsync(int personId, int ticketId)
    {
        // ---- Resolve staff ----
        var staffId = await _staff.GetStaffIdByPersonIdAsync(personId);
        if (staffId == null)
            return ApiResponse.Fail("No staff record linked to this account.");

        // ---- Load ticket ----
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ApiResponse.Fail("Ticket not found.");

        // ---- Must be the owner ----
        if (ticket.Staff_ID != staffId.Value)
            return ApiResponse.Fail("You can only cancel tickets you created.");

        // ---- Must be Open ----
        var openStatusId = await _tickets.GetStatusIdByNameAsync("Open");
        if (openStatusId == null)
            return ApiResponse.Fail("Status 'Open' is not configured.");

        if (ticket.Current_Status_ID != openStatusId.Value)
            return ApiResponse.Fail("You can only cancel tickets that are still Open.");

        // ---- Apply the cancellation ----
        var ok = await _tickets.CancelAsync(ticketId);
        if (!ok)
            return ApiResponse.Fail("Could not cancel ticket.");

        // ---- Audit log ----
        await _activity.LogAsync(
            ticketId,
            staffId.Value,
            "Cancelled",
            "Open",
            "Cancelled",
            "Reporter cancelled the ticket");

        return ApiResponse.Ok("Ticket cancelled.");
    }

    // CONFIRM FIXED — Reporter confirms the technician's fix
    public async Task<ApiResponse> ConfirmAsync(int personId, int ticketId)
    {
        // ---- Resolve staff ----
        var staffId = await _staff.GetStaffIdByPersonIdAsync(personId);
        if (staffId == null)
            return ApiResponse.Fail("No staff record linked to this account.");

        // ---- Load ticket ----
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ApiResponse.Fail("Ticket not found.");

        // ---- Must be the owner ----
        if (ticket.Staff_ID != staffId.Value)
            return ApiResponse.Fail("You can only confirm tickets you created.");

        // ---- Must be in Resolved status ----
        var resolvedId = await _tickets.GetStatusIdByNameAsync("Resolved");
        if (resolvedId == null)
            return ApiResponse.Fail("Status 'Resolved' is not configured.");

        if (ticket.Current_Status_ID != resolvedId.Value)
            return ApiResponse.Fail("Only tickets that are Resolved can be confirmed.");

        // ---- Move to Confirmed ----
        var confirmedId = await _tickets.GetStatusIdByNameAsync("Confirmed");
        if (confirmedId == null)
            return ApiResponse.Fail("Status 'Confirmed' is not configured.");

        await _tickets.UpdateStatusAsync(ticketId, confirmedId.Value, null, null);

        // ---- Audit log ----
        await _activity.LogAsync(
            ticketId,
            staffId.Value,
            "Confirmed",
            "Resolved",
            "Confirmed",
            "Reporter confirmed the issue is resolved");

        return ApiResponse.Ok("Ticket confirmed as resolved.");
    }

    // REJECT RESOLUTION — Reporter says "still not fixed"
    public async Task<ApiResponse> RejectResolutionAsync(int personId, int ticketId, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return ApiResponse.Fail("Please tell us what's still wrong.");

        if (note.Length > 200)
            return ApiResponse.Fail("Note must be 200 characters or less.");

        // ---- Resolve staff ----
        var staffId = await _staff.GetStaffIdByPersonIdAsync(personId);
        if (staffId == null)
            return ApiResponse.Fail("No staff record linked to this account.");

        // ---- Load ticket ----
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ApiResponse.Fail("Ticket not found.");

        // ---- Must be the owner ----
        if (ticket.Staff_ID != staffId.Value)
            return ApiResponse.Fail("You can only respond to tickets you created.");

        // ---- Must be in Resolved status ----
        var resolvedId = await _tickets.GetStatusIdByNameAsync("Resolved");
        if (resolvedId == null)
            return ApiResponse.Fail("Status 'Resolved' is not configured.");

        if (ticket.Current_Status_ID != resolvedId.Value)
            return ApiResponse.Fail("Only a Resolved ticket can be reopened by the reporter.");

        // ---- Move back to InProgress ----
        var inProgressId = await _tickets.GetStatusIdByNameAsync("InProgress");
        if (inProgressId == null)
            return ApiResponse.Fail("Status 'InProgress' is not configured.");

        // Clear Resolved_At so the ticket looks "in work" again
        await _tickets.UpdateStatusAsync(ticketId, inProgressId.Value, null, null);

        // ---- Audit log ----
        await _activity.LogAsync(
            ticketId,
            staffId.Value,
            "ResolutionRejected",
            "Resolved",
            "InProgress",
            note.Trim());

        return ApiResponse.Ok("Sent back to the technician.");
    }
}