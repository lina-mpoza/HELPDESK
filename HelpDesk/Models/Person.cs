namespace HelpDesk.Models;

public class Person
{
    public int Person_ID { get; set; }
    public string? First_Name { get; set; }
    public string? Last_Name { get; set; }
    public string? WorkEmail { get; set; }
    public string? Cell_No { get; set; }
    public string Username { get; set; } = "";
    public string? Password { get; set; }
    public DateTime? Registered_At { get; set; }
}