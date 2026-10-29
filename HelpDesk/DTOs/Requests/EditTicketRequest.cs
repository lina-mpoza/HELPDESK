namespace HelpDesk.DTOs.Requests;

public class EditTicketRequest
{
    public string IssueDescription { get; set; } = "";
    public string UrgencyLevel { get; set; } = "Medium";
}