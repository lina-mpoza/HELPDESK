namespace HelpDesk.DTOs.Requests;

public class CreateTicketRequest
{
    public string IssueDescription { get; set; } = "";
    public string UrgencyLevel { get; set; } = "Medium";  // Low | Medium | High | Critical
    public int? TechIssueId { get; set; }
    public int? ImageId { get; set; } 
    public int? VideoId { get; set; } 
}