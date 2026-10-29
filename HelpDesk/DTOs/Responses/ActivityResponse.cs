namespace HelpDesk.DTOs.Responses;

public class ActivityResponse
{
    public int ActivityId { get; set; }
    public int TicketId { get; set; }
    public string TicketSummary { get; set; } = "";   // short desc, for the system-wide view
    public int? PerformedByStaffId { get; set; }
    public string PerformedBy { get; set; } = "";   // full name
    public string Action { get; set; } = "";
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Note { get; set; }
    public DateTime Timestamp { get; set; }
}