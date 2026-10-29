namespace HelpDesk.Models;

public class IssueTicket
{
    public int Ticket_ID { get; set; }
    public DateTime Created_Date { get; set; }
    public int? Tech_Issue_ID { get; set; }
    public string? Urgency_Level { get; set; }
    public int? Video_ID { get; set; }
    public int? Image_ID { get; set; }
    public string? Issue_Description { get; set; }
    public int? Staff_ID { get; set; }
    public int? Current_Status_ID { get; set; }
    public DateTime? Resolved_At { get; set; }
    public DateTime? Closed_At { get; set; }
}