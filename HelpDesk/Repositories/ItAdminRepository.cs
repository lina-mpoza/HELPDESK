using HelpDesk.Data;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class ItAdminRepository : IItAdminRepository
{
    private readonly DbHelper _db;
    public ItAdminRepository(DbHelper db) => _db = db;

    public async Task<int?> GetAdminIdByStaffIdAsync(int staffId)
    {
        var r = await _db.ExecuteScalarAsync(
            "SELECT Admin_ID FROM ITAdmins WHERE Staff_ID = @sid",
            new SqlParameter("@sid", staffId));
        if (r == null || r == DBNull.Value) return null;
        return Convert.ToInt32(r);
    }

    public async Task<int?> GetAdminIdByPersonIdAsync(int personId)
    {
        var r = await _db.ExecuteScalarAsync(@"
            SELECT ia.Admin_ID
            FROM   ITAdmins ia
            JOIN   Staff s ON s.Staff_ID = ia.Staff_ID
            WHERE  s.Person_ID = @pid",
            new SqlParameter("@pid", personId));
        if (r == null || r == DBNull.Value) return null;
        return Convert.ToInt32(r);
    }
}