using HelpDesk.Data;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class GeneralStaffRepository : IGeneralStaffRepository
{
    private readonly DbHelper _db;
    public GeneralStaffRepository(DbHelper db) => _db = db;

    public async Task<int> CreateAsync(int staffId)
    {
        var id = await _db.ExecuteScalarAsync(@"
            INSERT INTO GeneralStaff (Staff_ID)
            OUTPUT INSERTED.GSID
            VALUES (@sid);",
            new SqlParameter("@sid", staffId));

        return Convert.ToInt32(id);
    }
}