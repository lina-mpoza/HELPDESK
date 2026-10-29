using HelpDesk.Data;
using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class TicketActivityRepository : ITicketActivityRepository
{
    private readonly DbHelper _db;
    public TicketActivityRepository(DbHelper db) => _db = db;

    public async Task LogAsync(
        int ticketId,
        int? performedByStaffId,
        string action,
        string? oldValue,
        string? newValue,
        string? note)
    {
        await _db.ExecuteAsync(@"
            INSERT INTO TicketActivity
                (Ticket_ID, PerformedBy_Staff, Action, Old_Value, New_Value, Note)
            VALUES
                (@tid, @pid, @action, @old, @new, @note);",
            new SqlParameter("@tid", ticketId),
            new SqlParameter("@pid", (object?)performedByStaffId ?? DBNull.Value),
            new SqlParameter("@action", action),
            new SqlParameter("@old", (object?)oldValue ?? DBNull.Value),
            new SqlParameter("@new", (object?)newValue ?? DBNull.Value),
            new SqlParameter("@note", (object?)note ?? DBNull.Value));
    }

    public async Task<List<ActivityResponse>> GetAllAsync(int limit = 200)
    {
        var table = await _db.QueryAsync(@"
            SELECT TOP (@lim)
                a.Activity_ID,
                a.Ticket_ID,
                LEFT(t.Issue_Description, 60) AS TicketSummary,
                a.PerformedBy_Staff,
                (p.First_Name + ' ' + p.Last_Name) AS PerformedBy,
                a.Action,
                a.Old_Value,
                a.New_Value,
                a.Note,
                a.Timestamp
            FROM TicketActivity a
            JOIN IssuesTickets t     ON t.Ticket_ID = a.Ticket_ID
            LEFT JOIN Staff s        ON s.Staff_ID  = a.PerformedBy_Staff
            LEFT JOIN Persons p      ON p.Person_ID = s.Person_ID
            ORDER BY a.Timestamp DESC;",
            new SqlParameter("@lim", limit));

        return MapRows(table);
    }

    public async Task<List<ActivityResponse>> GetForTicketAsync(int ticketId)
    {
        var table = await _db.QueryAsync(@"
            SELECT
                a.Activity_ID,
                a.Ticket_ID,
                LEFT(t.Issue_Description, 60) AS TicketSummary,
                a.PerformedBy_Staff,
                (p.First_Name + ' ' + p.Last_Name) AS PerformedBy,
                a.Action,
                a.Old_Value,
                a.New_Value,
                a.Note,
                a.Timestamp
            FROM TicketActivity a
            JOIN IssuesTickets t     ON t.Ticket_ID = a.Ticket_ID
            LEFT JOIN Staff s        ON s.Staff_ID  = a.PerformedBy_Staff
            LEFT JOIN Persons p      ON p.Person_ID = s.Person_ID
            WHERE a.Ticket_ID = @tid
            ORDER BY a.Timestamp ASC;",
            new SqlParameter("@tid", ticketId));

        return MapRows(table);
    }

    private static List<ActivityResponse> MapRows(System.Data.DataTable table)
    {
        var list = new List<ActivityResponse>();
        foreach (System.Data.DataRow r in table.Rows)
        {
            list.Add(new ActivityResponse
            {
                ActivityId = (int)r["Activity_ID"],
                TicketId = (int)r["Ticket_ID"],
                TicketSummary = r["TicketSummary"] as string ?? "",
                PerformedByStaffId = r["PerformedBy_Staff"] as int?,
                PerformedBy = (r["PerformedBy"] as string) ?? "(system)",
                Action = (string)r["Action"],
                OldValue = r["Old_Value"] as string,
                NewValue = r["New_Value"] as string,
                Note = r["Note"] as string,
                Timestamp = (DateTime)r["Timestamp"]
            });
        }
        return list;
    }
}