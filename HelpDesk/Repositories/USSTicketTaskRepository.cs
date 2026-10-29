using HelpDesk.Data;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class USSTicketTaskRepository : IUSSTicketTaskRepository
{
    private readonly DbHelper _db;
    public USSTicketTaskRepository(DbHelper db) => _db = db;

    public async Task<int> CreateAsync(int ticketId, int ussId, int statusId,
                                       string note, DateTime? dateSolved)
    {
        var id = await _db.ExecuteScalarAsync(@"
            INSERT INTO USSTicketsTasks (Ticket_ID, USS_ID, Status_ID, Note, Date_Solved)
            OUTPUT INSERTED.UTT_ID
            VALUES (@tid, @uid, @sid, @note, @ds);",
            new SqlParameter("@tid", ticketId),
            new SqlParameter("@uid", ussId),
            new SqlParameter("@sid", statusId),
            new SqlParameter("@note", (object?)note ?? DBNull.Value),
            new SqlParameter("@ds", (object?)dateSolved ?? DBNull.Value));

        return Convert.ToInt32(id);
    }
}