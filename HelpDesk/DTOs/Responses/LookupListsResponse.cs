namespace HelpDesk.DTOs.Responses;

public class LookupListsResponse
{
    public List<LookupItem> JobTitles           { get; set; } = new();
    public List<LookupItem> EmploymentStatuses  { get; set; } = new();
    public List<LookupItem> Municipalities      { get; set; } = new();
    public List<LookupItem> Departments         { get; set; } = new();
}

public class LookupItem
{
    public int    Id   { get; set; }
    public string Name { get; set; } = "";
}