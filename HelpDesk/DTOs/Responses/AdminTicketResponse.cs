namespace HelpDesk.DTOs.Responses;

public class AdminTicketResponse
{
    public int TicketId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string IssueDescription { get; set; } = "";
    public string UrgencyLevel { get; set; } = "";
    public int? CurrentStatusId { get; set; }
    public string CurrentStatus { get; set; } = "";
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Submitter
    public int? SubmittedByStaffId { get; set; }
    public string SubmittedBy { get; set; } = "";

    // Assignment (null if not assigned)
    public int? AssignedUssId { get; set; }
    public string AssignedTo { get; set; } = "";
    public DateTime? AssignedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public string AssignmentNote { get; set; } = "";
    public int? ImageId  { get; set; }  
}