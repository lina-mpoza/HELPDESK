using HelpDesk.Data;
using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class AdminService : IAdminService
{
    private readonly ITicketRepository _tickets;
    private readonly IUserSupportStaffRepository _uss;
    private readonly IItAdminRepository _admins;
    private readonly ITicketAssignmentRepository _assignments;
    private readonly IStaffRepository _staff;
    private readonly DbHelper _db;
    private readonly ITicketActivityRepository _activity;

    public AdminService(
        ITicketRepository tickets,
        IUserSupportStaffRepository uss,
        IItAdminRepository admins,
        ITicketAssignmentRepository assignments,
        IStaffRepository staff,
        DbHelper db,
        ITicketActivityRepository activity)
    {
        _tickets = tickets;
        _uss = uss;
        _admins = admins;
        _assignments = assignments;
        _staff = staff;
        _db = db;
        _activity = activity;
    }

    // GET ALL TICKETS
    public async Task<ApiResponse> GetAllTicketsAsync()
    {
        var rows = await _tickets.GetAllWithDetailsAsync();

        var result = rows.Select(r => new AdminTicketResponse
        {
            TicketId = r.Ticket_ID,
            CreatedDate = r.Created_Date,
            IssueDescription = r.Issue_Description ?? "",
            UrgencyLevel = r.Urgency_Level ?? "",
            CurrentStatusId = r.Current_Status_ID,
            CurrentStatus = r.Current_Status ?? "Unknown",
            ResolvedAt = r.Resolved_At,
            ClosedAt = r.Closed_At,
            SubmittedByStaffId = r.SubmittedStaffId,
            SubmittedBy = r.SubmittedByName ?? "(unknown)",
            AssignedUssId = r.AssignedUssId,
            AssignedTo = r.AssignedToName ?? "",
            AssignedAt = r.Assign_Date,
            Deadline = r.Deadline,
            AssignmentNote = r.AssignmentInstruction ?? "",
            ImageId = r.Image_ID
        }).ToList();

        return ApiResponse.Ok($"{result.Count} ticket(s).", result);
    }

    // GET TECHNICIANS
    public async Task<ApiResponse> GetTechniciansAsync()
    {
        var techs = await _uss.GetAllAsync();
        return ApiResponse.Ok($"{techs.Count} technician(s).", techs);
    }

    // ASSIGN TICKET TO TECHNICIAN
    public async Task<ApiResponse> AssignAsync(
        int adminPersonId,
        AssignTicketRequest req)
    {
        // Validation
        if (req.UssId <= 0)
            return ApiResponse.Fail("Please choose a technician.");

        if (string.IsNullOrWhiteSpace(req.Instruction))
            return ApiResponse.Fail("Please provide an instruction.");

        if (req.Deadline == default || req.Deadline.Year < 1753)
            return ApiResponse.Fail("Please provide a valid deadline.");

        // Resolve admin to ITAdmins.Admin_ID
        var adminId = await _admins.GetAdminIdByPersonIdAsync(adminPersonId);

        if (adminId == null)
            return ApiResponse.Fail("Could not identify the admin account.");

        // Reject duplicate assignment
        if (await _assignments.ExistsForTicketAsync(req.TicketId))
            return ApiResponse.Fail("This ticket is already assigned.");

        // Load the ticket
        var ticket = await _tickets.GetByIdAsync(req.TicketId);

        if (ticket == null)
            return ApiResponse.Fail("Ticket not found.");

        // Resolve municipality from the submitting staff
        int? municipalityId = null;

        if (ticket.Staff_ID != null)
        {
            municipalityId =
                await _staff.GetMunicipalityIdByStaffIdAsync(
                    ticket.Staff_ID.Value);
        }

        // Fallback municipality for testing
        if (municipalityId == null)
        {
            var fallback = await _db.ExecuteScalarAsync(
                "SELECT TOP 1 MID FROM Municipalities ORDER BY MID");

            if (fallback == null || fallback == DBNull.Value)
            {
                return ApiResponse.Fail(
                    "No municipality configured. Please insert a row in the Municipalities table.");
            }

            municipalityId = Convert.ToInt32(fallback);
        }

        // Insert assignment
        await _assignments.CreateAsync(
            adminId.Value,
            municipalityId.Value,
            req.UssId,
            req.TicketId,
            req.Instruction.Trim(),
            req.Deadline);

        // Move ticket to "Assigned"
        var statusId =
            await _tickets.GetStatusIdByNameAsync("Assigned");

        if (statusId == null)
            return ApiResponse.Fail(
                "Status 'Assigned' is not configured.");

        await _tickets.UpdateStatusAsync(
            req.TicketId,
            statusId.Value,
            null,
            null);

        // Log activity
        await _activity.LogAsync(
            req.TicketId,
            await GetStaffIdFromAdminPersonIdAsync(adminPersonId),
            "Assigned",
            "Open",
            "Assigned",
            req.Instruction.Trim());

        return ApiResponse.Ok("Ticket assigned.");
    }

    // CLOSE TICKET (admin verifies the resolution)
    public async Task<ApiResponse> CloseTicketAsync(int adminPersonId, int ticketId)
    {
        // Load ticket
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ApiResponse.Fail("Ticket not found.");

        // Resolve statuses
        var resolvedId = await _tickets.GetStatusIdByNameAsync("Resolved");
        var confirmedId = await _tickets.GetStatusIdByNameAsync("Confirmed");
        var closedId = await _tickets.GetStatusIdByNameAsync("Closed");

        if (resolvedId == null || confirmedId == null || closedId == null)
            return ApiResponse.Fail("Required statuses are not configured.");

        // Guard: must be Resolved or Confirmed
        bool isResolved = ticket.Current_Status_ID == resolvedId.Value;
        bool isConfirmed = ticket.Current_Status_ID == confirmedId.Value;

        if (!isResolved && !isConfirmed)
            return ApiResponse.Fail("Only Resolved or Confirmed tickets can be closed.");

        // ---- Update ticket ----
        await _tickets.UpdateStatusAsync(ticketId, closedId.Value, null, DateTime.UtcNow);

        // ---- Audit ----
        var staffId = await GetStaffIdFromAdminPersonIdAsync(adminPersonId);

        string note = isConfirmed
            ? "Admin closed after staff confirmation"
            : "Admin closed without staff confirmation (override)";

        string oldStatus = isConfirmed ? "Confirmed" : "Resolved";

        await _activity.LogAsync(
            ticketId,
            staffId,
            "Closed",
            oldStatus,
            "Closed",
            note);

        return ApiResponse.Ok("Ticket closed.");
    }

    // REOPEN TICKET (admin rejects the resolution)
    public async Task<ApiResponse> ReopenTicketAsync(
        int adminPersonId,
        int ticketId)
    {
        var inProgress =
            await _tickets.GetStatusIdByNameAsync("InProgress");

        if (inProgress == null)
            return ApiResponse.Fail(
                "Status 'InProgress' is not configured.");

        await _tickets.UpdateStatusAsync(
            ticketId,
            inProgress.Value,
            null,
            null);

        // Log activity
        await _activity.LogAsync(
            ticketId,
            await GetStaffIdFromAdminPersonIdAsync(adminPersonId),
            "Reopened",
            "Resolved",
            "InProgress",
            "Admin reopened the ticket");

        return ApiResponse.Ok("Ticket reopened.");
    }

    // Convert Admin Person_ID → Staff_ID
    // TicketActivity.PerformedBy_Staff requires Staff_ID
    private async Task<int?> GetStaffIdFromAdminPersonIdAsync(int personId)
    {
        var result = await _db.ExecuteScalarAsync(
            "SELECT Staff_ID FROM Staff WHERE Person_ID = @pid",
            new Microsoft.Data.SqlClient.SqlParameter("@pid", personId));

        if (result == null || result == DBNull.Value)
            return null;

        return Convert.ToInt32(result);
    }
}