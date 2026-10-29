namespace HelpDesk.DTOs.Requests;

public class CreateTechnicianRequest
{
    // Personal
    public string FirstName { get; set; } = "";
    public string LastName  { get; set; } = "";
    public string WorkEmail { get; set; } = "";
    public string CellNo    { get; set; } = "";

    // Account
    public string Username  { get; set; } = "";
    public string Password  { get; set; } = "";
    public string StaffNo   { get; set; } = "";

    // Technician
    public string OfficeNo       { get; set; } = "";
    public string OfficeLocation { get; set; } = "";

    // Organisation
    public int JobId       { get; set; }   // JobTitles.Job_ID
    public int EStatusId   { get; set; }   // EmploymentStatuses.EStatus_ID
    public int MID         { get; set; }   // Municipalities.MID
    public int DepId       { get; set; }   // Departments.DepID
}