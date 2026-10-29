using HelpDesk.Models;

namespace HelpDesk.Repositories.Interfaces;

public interface ITicketRepository
{
    Task<int> CreateAsync(IssueTicket ticket);
    Task<List<IssueTicket>> GetByStaffIdAsync(int staffId);
    Task<IssueTicket?> GetByIdAsync(int ticketId);
    Task<string?> GetStatusNameAsync(int statusId);
    Task<int?> GetStatusIdByNameAsync(string statusName);
    Task UpdateStatusAsync(int ticketId, int statusId, DateTime? resolvedAt, DateTime? closedAt);
    Task<List<AdminTicketRow>> GetAllWithDetailsAsync();
    Task<List<TechnicianTicketRow>> GetByUssIdAsync(int ussId);
    Task<bool> UpdateDescriptionAndUrgencyAsync(int ticketId, string description, string urgency);
    Task<bool> CancelAsync(int ticketId);
}

// A flat row for the admin grid (joined data)
public class AdminTicketRow
{
    public int Ticket_ID { get; set; }
    public DateTime Created_Date { get; set; }
    public string? Issue_Description { get; set; }
    public string? Urgency_Level { get; set; }
    public int? Current_Status_ID { get; set; }
    public string? Current_Status { get; set; }
    public DateTime? Resolved_At { get; set; }
    public DateTime? Closed_At { get; set; }
    public int? SubmittedStaffId { get; set; }
    public string? SubmittedByName { get; set; }
    public int? AssignedUssId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? Assign_Date { get; set; }
    public DateTime? Deadline { get; set; }
    public string? AssignmentInstruction { get; set; }
    public int?   Image_ID    { get; set; }
}

// A flat row for the technician's assigned tickets 
public class TechnicianTicketRow
{
    public int Ticket_ID { get; set; }
    public DateTime Created_Date { get; set; }
    public string? Issue_Description { get; set; }
    public string? Urgency_Level { get; set; }
    public int? Current_Status_ID { get; set; }
    public string? Current_Status { get; set; }
    public string? SubmittedByName { get; set; }
    public DateTime? Assign_Date { get; set; }
    public DateTime? Deadline { get; set; }
    public string? AssignmentInstruction { get; set; }
    public DateTime? Resolved_At { get; set; }
    public string? ResolutionNote { get; set; }
    public int?   Image_ID    { get; set; }
}