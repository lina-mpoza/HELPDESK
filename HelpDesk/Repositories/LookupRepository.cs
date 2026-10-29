using HelpDesk.Data;
using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class LookupRepository : ILookupRepository
{
    private readonly DbHelper _db;
    public LookupRepository(DbHelper db) => _db = db;

    public async Task<List<LookupItem>> GetJobTitlesAsync()
    {
        var table = await _db.QueryAsync(
            "SELECT Job_ID, Job_Title FROM JobTitles ORDER BY Job_Title");

        return table.Rows.Cast<System.Data.DataRow>()
            .Select(r => new LookupItem
            {
                Id   = (int)r["Job_ID"],
                Name = r["Job_Title"] as string ?? ""
            }).ToList();
    }

    public async Task<List<LookupItem>> GetEmploymentStatusesAsync()
    {
        var table = await _db.QueryAsync(
            "SELECT EStatus_ID, EStatus_Name FROM EmploymentStatuses ORDER BY EStatus_Name");

        return table.Rows.Cast<System.Data.DataRow>()
            .Select(r => new LookupItem
            {
                Id   = (int)r["EStatus_ID"],
                Name = r["EStatus_Name"] as string ?? ""
            }).ToList();
    }

    public async Task<LookupItem?> GetAdminMunicipalityAsync(int adminPersonId)
    {
        // Person → Staff → MunicipalitiesDepartments → Municipalities
        var table = await _db.QueryAsync(@"
            SELECT TOP 1 m.MID, m.Municipal_Name
            FROM   Persons p
            JOIN   Staff s     ON s.Person_ID = p.Person_ID
            JOIN   MunicipalitiesDepartments md ON md.MDID = s.MDID
            JOIN   Municipalities m             ON m.MID  = md.MID
            WHERE  p.Person_ID = @pid",
            new SqlParameter("@pid", adminPersonId));

        if (table.Rows.Count == 0) return null;
        var r = table.Rows[0];

        return new LookupItem
        {
            Id   = (int)r["MID"],
            Name = r["Municipal_Name"] as string ?? ""
        };
    }

    public async Task<List<LookupItem>> GetDepartmentsForMunicipalityAsync(int municipalityId)
    {
        var table = await _db.QueryAsync(@"
            SELECT DISTINCT d.DepID, d.DepName
            FROM   MunicipalitiesDepartments md
            JOIN   Departments d ON d.DepID = md.DepID
            WHERE  md.MID = @mid
            ORDER  BY d.DepName",
            new SqlParameter("@mid", municipalityId));

        return table.Rows.Cast<System.Data.DataRow>()
            .Select(r => new LookupItem
            {
                Id   = (int)r["DepID"],
                Name = r["DepName"] as string ?? ""
            }).ToList();
    }
}