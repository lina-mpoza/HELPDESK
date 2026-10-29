using HelpDesk.Data;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class TicketAssignmentRepository : ITicketAssignmentRepository
{
    private readonly DbHelper _db;
    public TicketAssignmentRepository(DbHelper db) => _db = db;

    public async Task<int> CreateAsync(int adminId, int municipalityId, int ussId,
                                       int ticketId, string instruction, DateTime deadline)
    {
        var id = await _db.ExecuteScalarAsync(@"
            INSERT INTO TicketAssignment
                (Assignment_Instruction, Assign_Date, Deadline,
                 Admin_ID, MID, USS_ID, Ticket_ID)
            OUTPUT INSERTED.Assignment_ID
            VALUES (@inst, SYSUTCDATETIME(), @dl, @aid, @mid, @uss, @tid);",
            new SqlParameter("@inst", instruction),
            new SqlParameter("@dl", deadline),
            new SqlParameter("@aid", adminId),
            new SqlParameter("@mid", municipalityId),
            new SqlParameter("@uss", ussId),
            new SqlParameter("@tid", ticketId));

        return Convert.ToInt32(id);
    }

    public async Task<bool> ExistsForTicketAsync(int ticketId)
    {
        var count = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM TicketAssignment WHERE Ticket_ID = @tid",
            new SqlParameter("@tid", ticketId));

        return Convert.ToInt32(count) > 0;
    }
}