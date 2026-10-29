namespace HelpDesk.DTOs.Requests;

public class AssignTicketRequest
{
    public int TicketId { get; set; }
    public int UssId { get; set; }
    public string Instruction { get; set; } = "";
    public DateTime Deadline { get; set; }
}