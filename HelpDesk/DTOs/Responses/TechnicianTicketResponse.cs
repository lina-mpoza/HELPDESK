namespace HelpDesk.DTOs.Responses;

public class TechnicianTicketResponse
{
    public int TicketId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string IssueDescription { get; set; } = "";
    public string UrgencyLevel { get; set; } = "";
    public int? CurrentStatusId { get; set; }
    public string CurrentStatus { get; set; } = "";
    public string SubmittedBy { get; set; } = "";

    public DateTime? AssignDate { get; set; }
    public DateTime? Deadline { get; set; }
    public string AssignmentNote { get; set; } = "";

    public DateTime? ResolvedAt { get; set; }
    public string ResolutionNote { get; set; } = "";
    public int?  ImageId  { get; set; }
}