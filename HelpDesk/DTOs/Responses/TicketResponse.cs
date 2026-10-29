namespace HelpDesk.DTOs.Responses;

public class TicketResponse
{
    public int TicketId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string IssueDescription { get; set; } = "";
    public string UrgencyLevel { get; set; } = "";
    public int? CurrentStatusId { get; set; }
    public string CurrentStatus { get; set; } = "";
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ImageId { get; set; } 
}