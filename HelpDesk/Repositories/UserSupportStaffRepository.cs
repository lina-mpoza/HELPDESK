using HelpDesk.Data;
using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class UserSupportStaffRepository : IUserSupportStaffRepository
{
    private readonly DbHelper _db;
    public UserSupportStaffRepository(DbHelper db) => _db = db;

    public async Task<List<TechnicianResponse>> GetAllAsync()
    {
        var table = await _db.QueryAsync(@"
            SELECT uss.USS_ID,
                   st.Staff_ID,
                   (p.First_Name + ' ' + p.Last_Name) AS FullName,
                   ISNULL(uss.Office_No, '')       AS Office_No,
                   ISNULL(uss.Office_Location, '') AS Office_Location
            FROM   UserSupportStaff uss
            JOIN   Staff   st ON st.Staff_ID  = uss.Staff_ID
            JOIN   Persons p  ON p.Person_ID  = st.Person_ID
            ORDER  BY p.First_Name, p.Last_Name;");

        var list = new List<TechnicianResponse>();
        foreach (System.Data.DataRow r in table.Rows)
        {
            list.Add(new TechnicianResponse
            {
                UssId = (int)r["USS_ID"],
                StaffId = (int)r["Staff_ID"],
                FullName = (string)r["FullName"],
                OfficeNo = (string)r["Office_No"],
                OfficeLocation = (string)r["Office_Location"]
            });
        }
        return list;
    }

    public async Task<int?> GetUssIdByStaffIdAsync(int staffId)
    {
        var r = await _db.ExecuteScalarAsync(
            "SELECT USS_ID FROM UserSupportStaff WHERE Staff_ID = @sid",
            new SqlParameter("@sid", staffId));
        if (r == null || r == DBNull.Value) return null;
        return Convert.ToInt32(r);
    }

    public async Task<int?> GetStaffIdByUssIdAsync(int ussId)
    {
        var r = await _db.ExecuteScalarAsync(
            "SELECT Staff_ID FROM UserSupportStaff WHERE USS_ID = @uid",
            new SqlParameter("@uid", ussId));
        if (r == null || r == DBNull.Value) return null;
        return Convert.ToInt32(r);
    }

    public async Task<int?> GetUssIdByPersonIdAsync(int personId)
    {
        var r = await _db.ExecuteScalarAsync(@"
        SELECT uss.USS_ID
        FROM   UserSupportStaff uss
        JOIN   Staff s ON s.Staff_ID = uss.Staff_ID
        WHERE  s.Person_ID = @pid",
            new SqlParameter("@pid", personId));

        if (r == null || r == DBNull.Value) return null;
        return Convert.ToInt32(r);
    }
}