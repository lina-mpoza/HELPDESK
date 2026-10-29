using HelpDesk.Data;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly DbHelper _db;
    public StaffRepository(DbHelper db) => _db = db;

    public async Task<int> CreateAsync(int personId, string? staffNo)
    {
        var id = await _db.ExecuteScalarAsync(@"
            INSERT INTO Staff (Person_ID, StaffNo)
            OUTPUT INSERTED.Staff_ID
            VALUES (@pid, @sn);",
            new SqlParameter("@pid", personId),
            new SqlParameter("@sn", (object?)staffNo ?? DBNull.Value));

        return Convert.ToInt32(id);
    }

    public async Task<int?> GetStaffIdByPersonIdAsync(int personId)
    {
        var result = await _db.ExecuteScalarAsync(
            "SELECT Staff_ID FROM Staff WHERE Person_ID = @pid",
            new SqlParameter("@pid", personId));

        if (result == null || result == DBNull.Value) return null;
        return Convert.ToInt32(result);
    }

    public async Task<string> GetRoleByStaffIdAsync(int staffId)
    {
        // Check in order of privilege.
        var isAdmin = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM ITAdmins WHERE Staff_ID = @sid",
            new SqlParameter("@sid", staffId));
        if (Convert.ToInt32(isAdmin) > 0) return "Admin";

        var isTech = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM UserSupportStaff WHERE Staff_ID = @sid",
            new SqlParameter("@sid", staffId));
        if (Convert.ToInt32(isTech) > 0) return "Technician";

        return "GeneralStaff";
    }


    public async Task<int?> GetMunicipalityIdByStaffIdAsync(int staffId)
    {
        var result = await _db.ExecuteScalarAsync(@"
        SELECT md.MID
        FROM   Staff s
        JOIN   MunicipalitiesDepartments md ON md.MDID = s.MDID
        WHERE  s.Staff_ID = @sid",
            new SqlParameter("@sid", staffId));

        if (result == null || result == DBNull.Value) return null;
        return Convert.ToInt32(result);
    }
}