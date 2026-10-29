using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly DbHelper _db;
    public TicketRepository(DbHelper db) => _db = db;

    public async Task<int> CreateAsync(IssueTicket t)
    {
        var id = await _db.ExecuteScalarAsync(@"
            INSERT INTO IssuesTickets
                (Tech_Issue_ID, Urgency_Level, Video_ID, Image_ID,
                 Issue_Description, Staff_ID, Current_Status_ID)
            OUTPUT INSERTED.Ticket_ID
            VALUES (@ti, @ul, @vi, @ii, @desc, @sid, @cs);",
            new SqlParameter("@ti", (object?)t.Tech_Issue_ID ?? DBNull.Value),
            new SqlParameter("@ul", (object?)t.Urgency_Level ?? DBNull.Value),
            new SqlParameter("@vi", (object?)t.Video_ID ?? DBNull.Value),
            new SqlParameter("@ii", (object?)t.Image_ID ?? DBNull.Value),
            new SqlParameter("@desc", (object?)t.Issue_Description ?? DBNull.Value),
            new SqlParameter("@sid", (object?)t.Staff_ID ?? DBNull.Value),
            new SqlParameter("@cs", (object?)t.Current_Status_ID ?? DBNull.Value));

        return Convert.ToInt32(id);
    }

    public async Task<List<IssueTicket>> GetByStaffIdAsync(int staffId)
    {
        var table = await _db.QueryAsync(@"
            SELECT Ticket_ID, Created_Date, Tech_Issue_ID, Urgency_Level,
                   Video_ID, Image_ID, Issue_Description, Staff_ID,
                   Current_Status_ID, Resolved_At, Closed_At
            FROM   IssuesTickets
            WHERE  Staff_ID = @sid
            ORDER  BY Created_Date DESC",
            new SqlParameter("@sid", staffId));

        var list = new List<IssueTicket>();
        foreach (System.Data.DataRow r in table.Rows)
        {
            list.Add(new IssueTicket
            {
                Ticket_ID = (int)r["Ticket_ID"],
                Created_Date = (DateTime)r["Created_Date"],
                Tech_Issue_ID = r["Tech_Issue_ID"] as int?,
                Urgency_Level = r["Urgency_Level"] as string,
                Video_ID = r["Video_ID"] as int?,
                Image_ID = r["Image_ID"] as int?,
                Issue_Description = r["Issue_Description"] as string,
                Staff_ID = r["Staff_ID"] as int?,
                Current_Status_ID = r["Current_Status_ID"] as int?,
                Resolved_At = r["Resolved_At"] as DateTime?,
                Closed_At = r["Closed_At"] as DateTime?
            });
        }
        return list;
    }

    public async Task<IssueTicket?> GetByIdAsync(int ticketId)
    {
        var table = await _db.QueryAsync(
            "SELECT * FROM IssuesTickets WHERE Ticket_ID = @id",
            new SqlParameter("@id", ticketId));

        if (table.Rows.Count == 0) return null;
        var r = table.Rows[0];

        return new IssueTicket
        {
            Ticket_ID = (int)r["Ticket_ID"],
            Created_Date = (DateTime)r["Created_Date"],
            Tech_Issue_ID = r["Tech_Issue_ID"] as int?,
            Urgency_Level = r["Urgency_Level"] as string,
            Video_ID = r["Video_ID"] as int?,
            Image_ID = r["Image_ID"] as int?,
            Issue_Description = r["Issue_Description"] as string,
            Staff_ID = r["Staff_ID"] as int?,
            Current_Status_ID = r["Current_Status_ID"] as int?,
            Resolved_At = r["Resolved_At"] as DateTime?,
            Closed_At = r["Closed_At"] as DateTime?
        };
    }

    public async Task<string?> GetStatusNameAsync(int statusId)
    {
        var result = await _db.ExecuteScalarAsync(
            "SELECT Status_Name FROM Statuses WHERE Status_ID = @id",
            new SqlParameter("@id", statusId));
        return result as string;
    }

    public async Task<int?> GetStatusIdByNameAsync(string statusName)
    {
        var result = await _db.ExecuteScalarAsync(
            "SELECT Status_ID FROM Statuses WHERE Status_Name = @n",
            new SqlParameter("@n", statusName));
        if (result == null || result == DBNull.Value) return null;
        return Convert.ToInt32(result);
    }

    public async Task UpdateStatusAsync(int ticketId, int statusId,
                                    DateTime? resolvedAt, DateTime? closedAt)
    {
        await _db.ExecuteAsync(@"
        UPDATE IssuesTickets
           SET Current_Status_ID = @sid,
               Resolved_At       = @ra,
               Closed_At         = @ca
         WHERE Ticket_ID = @tid;",
            new SqlParameter("@sid", statusId),
            new SqlParameter("@ra", (object?)resolvedAt ?? DBNull.Value),
            new SqlParameter("@ca", (object?)closedAt ?? DBNull.Value),
            new SqlParameter("@tid", ticketId));
    }

    public async Task<List<AdminTicketRow>> GetAllWithDetailsAsync()
{
    var table = await _db.QueryAsync(@"
        SELECT
            t.Ticket_ID,
            t.Created_Date,
            t.Issue_Description,
            t.Urgency_Level,
            t.Current_Status_ID,
            s.Status_Name                         AS Current_Status,
            t.Resolved_At,
            t.Closed_At,
            t.Staff_ID                            AS SubmittedStaffId,
            (p.First_Name + ' ' + p.Last_Name)    AS SubmittedByName,
            a.USS_ID                              AS AssignedUssId,
            (pt.First_Name + ' ' + pt.Last_Name)  AS AssignedToName,
            a.Assign_Date,
            a.Deadline,
            a.Assignment_Instruction,
            t.Image_ID
        FROM IssuesTickets t
        LEFT JOIN Statuses           s   ON s.Status_ID   = t.Current_Status_ID
        LEFT JOIN Staff              st  ON st.Staff_ID   = t.Staff_ID
        LEFT JOIN Persons            p   ON p.Person_ID   = st.Person_ID
        LEFT JOIN TicketAssignment   a   ON a.Ticket_ID   = t.Ticket_ID
        LEFT JOIN UserSupportStaff   uss ON uss.USS_ID    = a.USS_ID
        LEFT JOIN Staff              st2 ON st2.Staff_ID  = uss.Staff_ID
        LEFT JOIN Persons            pt  ON pt.Person_ID  = st2.Person_ID
        ORDER BY t.Created_Date DESC;");

    var list = new List<AdminTicketRow>();
    foreach (System.Data.DataRow r in table.Rows)
    {
        list.Add(new AdminTicketRow
        {
            Ticket_ID             = (int)r["Ticket_ID"],
            Created_Date          = (DateTime)r["Created_Date"],
            Issue_Description     = r["Issue_Description"]      as string,
            Urgency_Level         = r["Urgency_Level"]          as string,
            Current_Status_ID     = r["Current_Status_ID"]      as int?,
            Current_Status        = r["Current_Status"]         as string,
            Resolved_At           = r["Resolved_At"]            as DateTime?,
            Closed_At             = r["Closed_At"]              as DateTime?,
            SubmittedStaffId      = r["SubmittedStaffId"]       as int?,
            SubmittedByName       = r["SubmittedByName"]        as string,
            AssignedUssId         = r["AssignedUssId"]          as int?,
            AssignedToName        = r["AssignedToName"]         as string,
            Assign_Date           = r["Assign_Date"]            as DateTime?,
            Deadline              = r["Deadline"]               as DateTime?,
            AssignmentInstruction = r["Assignment_Instruction"] as string,
            Image_ID              = r["Image_ID"]               as int?
        });
    }
    return list;
    }
    public async Task<List<TechnicianTicketRow>> GetByUssIdAsync(int ussId)
{
    var table = await _db.QueryAsync(@"
        SELECT
            t.Ticket_ID,
            t.Created_Date,
            t.Issue_Description,
            t.Urgency_Level,
            t.Current_Status_ID,
            s.Status_Name                          AS Current_Status,
            (p.First_Name + ' ' + p.Last_Name)     AS SubmittedByName,
            a.Assign_Date,
            a.Deadline,
            a.Assignment_Instruction,
            t.Resolved_At,
            utt.Note                               AS ResolutionNote,
            t.Image_ID
        FROM TicketAssignment a
        JOIN IssuesTickets t            ON t.Ticket_ID   = a.Ticket_ID
        LEFT JOIN Statuses s            ON s.Status_ID   = t.Current_Status_ID
        LEFT JOIN Staff   st            ON st.Staff_ID   = t.Staff_ID
        LEFT JOIN Persons p             ON p.Person_ID   = st.Person_ID
        LEFT JOIN USSTicketsTasks utt   ON utt.Ticket_ID = t.Ticket_ID
                                        AND utt.USS_ID    = a.USS_ID
        WHERE a.USS_ID = @uid
        ORDER BY a.Assign_Date DESC;",
        new Microsoft.Data.SqlClient.SqlParameter("@uid", ussId));

    var list = new List<TechnicianTicketRow>();
    foreach (System.Data.DataRow r in table.Rows)
    {
        list.Add(new TechnicianTicketRow
        {
            Ticket_ID             = (int)r["Ticket_ID"],
            Created_Date          = (DateTime)r["Created_Date"],
            Issue_Description     = r["Issue_Description"]      as string,
            Urgency_Level         = r["Urgency_Level"]          as string,
            Current_Status_ID     = r["Current_Status_ID"]      as int?,
            Current_Status        = r["Current_Status"]         as string,
            SubmittedByName       = r["SubmittedByName"]        as string,
            Assign_Date           = r["Assign_Date"]            as DateTime?,
            Deadline              = r["Deadline"]               as DateTime?,
            AssignmentInstruction = r["Assignment_Instruction"] as string,
            Resolved_At           = r["Resolved_At"]            as DateTime?,
            ResolutionNote        = r["ResolutionNote"]         as string,
            Image_ID              = r["Image_ID"]               as int?
        });
    }
    return list;
}
    public async Task<bool> UpdateDescriptionAndUrgencyAsync(int ticketId, string description, string urgency)
    {
        var rows = await _db.ExecuteAsync(@"
        UPDATE IssuesTickets
           SET Issue_Description = @desc,
               Urgency_Level     = @urg
         WHERE Ticket_ID = @tid;",
            new SqlParameter("@desc", description),
            new SqlParameter("@urg", urgency),
            new SqlParameter("@tid", ticketId));

        return rows > 0;
    }
    public async Task<bool> CancelAsync(int ticketId)
    {
        // Get the "Cancelled" status id
        var statusId = await GetStatusIdByNameAsync("Cancelled");
        if (statusId == null) return false;

        var rows = await _db.ExecuteAsync(@"
        UPDATE IssuesTickets
           SET Current_Status_ID = @sid
         WHERE Ticket_ID = @tid;",
            new SqlParameter("@sid", statusId.Value),
            new SqlParameter("@tid", ticketId));

        return rows > 0;
    }
}