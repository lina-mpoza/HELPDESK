using HelpDesk.Data;
using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses; 
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class TechnicianAdminRepository : ITechnicianAdminRepository
{
    private readonly DbHelper _db;
    public TechnicianAdminRepository(DbHelper db) => _db = db;

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var c = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM Persons WHERE Username = @u",
            new SqlParameter("@u", username));
        return Convert.ToInt32(c) > 0;
    }

    public async Task<bool> StaffNoExistsAsync(string staffNo)
    {
        var c = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM Staff WHERE StaffNo = @sn",
            new SqlParameter("@sn", staffNo));
        return Convert.ToInt32(c) > 0;
    }

    public async Task<(bool ok, string message, int personId)> CreateTechnicianAsync(CreateTechnicianRequest req)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        try
        {
            // 0. Ensure MunicipalitiesDepartments row exists (MID + DepID)
            int mdid;
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 MDID FROM MunicipalitiesDepartments
                  WHERE MID = @mid AND DepID = @did", conn, tx))
            {
                cmd.Parameters.AddWithValue("@mid", req.MID);
                cmd.Parameters.AddWithValue("@did", req.DepId);
                var existing = await cmd.ExecuteScalarAsync();

                if (existing != null && existing != DBNull.Value)
                {
                    mdid = Convert.ToInt32(existing);
                }
                else
                {
                    using var ins = new SqlCommand(
                        @"INSERT INTO MunicipalitiesDepartments (MID, DepID)
                          OUTPUT INSERTED.MDID
                          VALUES (@mid, @did)", conn, tx);
                    ins.Parameters.AddWithValue("@mid", req.MID);
                    ins.Parameters.AddWithValue("@did", req.DepId);
                    mdid = Convert.ToInt32(await ins.ExecuteScalarAsync());
                }
            }

            // 1. Insert Person
            int personId;
            using (var cmd = new SqlCommand(
                @"INSERT INTO Persons
                    (First_Name, Last_Name, WorkEmail, Cell_No, Username, Password)
                  OUTPUT INSERTED.Person_ID
                  VALUES (@fn, @ln, @em, @cn, @un, @pw)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@fn", req.FirstName.Trim());
                cmd.Parameters.AddWithValue("@ln", req.LastName.Trim());
                cmd.Parameters.AddWithValue("@em", (object?)req.WorkEmail?.Trim() ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cn", (object?)req.CellNo?.Trim()    ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@un", req.Username.Trim());
                cmd.Parameters.AddWithValue("@pw", req.Password);
                personId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // 2. Insert Staff
            int staffId;
            using (var cmd = new SqlCommand(
                @"INSERT INTO Staff (Person_ID, EStatus_ID, MDID, StaffNo, Job_ID)
                  OUTPUT INSERTED.Staff_ID
                  VALUES (@pid, @est, @mdid, @sn, @jid)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@pid",  personId);
                cmd.Parameters.AddWithValue("@est",  req.EStatusId);
                cmd.Parameters.AddWithValue("@mdid", mdid);
                cmd.Parameters.AddWithValue("@sn",   req.StaffNo.Trim());
                cmd.Parameters.AddWithValue("@jid",  req.JobId);
                staffId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // 3. Insert UserSupportStaff (Technician)
            using (var cmd = new SqlCommand(
                @"INSERT INTO UserSupportStaff (Staff_ID, Office_No, Office_Location)
                  VALUES (@sid, @on, @ol)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@sid", staffId);
                cmd.Parameters.AddWithValue("@on", (object?)req.OfficeNo?.Trim()       ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ol", (object?)req.OfficeLocation?.Trim() ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }

            tx.Commit();
            return (true, "Technician created successfully.", personId);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return (false, ex.Message, 0);
        }
    }

    public async Task<List<AdminUserListItem>> GetAllUsersAsync()
{
    var table = await _db.QueryAsync(@"
        SELECT
            p.Person_ID,
            p.First_Name,
            p.Last_Name,
            p.Username,
            p.WorkEmail,
            p.Cell_No,
            p.Registered_At,
            s.Staff_ID,
            s.StaffNo,
            uss.Office_No,
            uss.Office_Location,
            CASE
                WHEN ia.Admin_ID  IS NOT NULL THEN 'Admin'
                WHEN uss.USS_ID   IS NOT NULL THEN 'Technician'
                WHEN gs.GSID      IS NOT NULL THEN 'GeneralStaff'
                ELSE 'No role'
            END AS Role
        FROM Persons p
        LEFT JOIN Staff             s   ON s.Person_ID = p.Person_ID
        LEFT JOIN ITAdmins          ia  ON ia.Staff_ID = s.Staff_ID
        LEFT JOIN UserSupportStaff  uss ON uss.Staff_ID = s.Staff_ID
        LEFT JOIN GeneralStaff      gs  ON gs.Staff_ID  = s.Staff_ID
        ORDER BY p.Person_ID DESC;");

    var list = new List<AdminUserListItem>();
    foreach (System.Data.DataRow r in table.Rows)
    {
        list.Add(new AdminUserListItem
        {
            PersonId       = (int)r["Person_ID"],
            StaffId        = r["Staff_ID"]        as int?,
            FullName       = $"{r["First_Name"]} {r["Last_Name"]}".Trim(),
            Username       = (string)r["Username"],
            WorkEmail      = r["WorkEmail"]       as string ?? "",
            CellNo         = r["Cell_No"]         as string ?? "",
            StaffNo        = r["StaffNo"]         as string ?? "",
            Role           = (string)r["Role"],
            OfficeNo       = r["Office_No"]       as string ?? "",
            OfficeLocation = r["Office_Location"] as string ?? "",
            RegisteredAt   = r["Registered_At"]   as DateTime?
        });
    }
    return list;
}
    public async Task<ProfileResponse?> GetProfileAsync(int personId)
    {
        var table = await _db.QueryAsync(@"
        SELECT
            p.Person_ID,
            p.First_Name,
            p.Last_Name,
            p.Username,
            p.WorkEmail,
            p.Cell_No,
            s.StaffNo
        FROM Persons p
        LEFT JOIN Staff s ON s.Person_ID = p.Person_ID
        WHERE p.Person_ID = @pid",
            new SqlParameter("@pid", personId));

        if (table.Rows.Count == 0) return null;
        var r = table.Rows[0];

        return new ProfileResponse
        {
            PersonId = (int)r["Person_ID"],
            FirstName = r["First_Name"] as string ?? "",
            LastName = r["Last_Name"] as string ?? "",
            Username = (string)r["Username"],
            WorkEmail = r["WorkEmail"] as string ?? "",
            CellNo = r["Cell_No"] as string ?? "",
            StaffNo = r["StaffNo"] as string ?? ""
        };
    }

    public async Task<bool> UpdateProfileAsync(int personId, UpdateProfileRequest req)
    {
        // If NewPassword is supplied, update it too
        if (!string.IsNullOrWhiteSpace(req.NewPassword))
        {
            var rows = await _db.ExecuteAsync(@"
            UPDATE Persons
               SET First_Name = @fn,
                   Last_Name  = @ln,
                   WorkEmail  = @em,
                   Cell_No    = @cn,
                   Password   = @pw
             WHERE Person_ID = @pid",
                new SqlParameter("@fn", req.FirstName.Trim()),
                new SqlParameter("@ln", req.LastName.Trim()),
                new SqlParameter("@em", (object?)req.WorkEmail?.Trim() ?? DBNull.Value),
                new SqlParameter("@cn", (object?)req.CellNo?.Trim() ?? DBNull.Value),
                new SqlParameter("@pw", req.NewPassword),
                new SqlParameter("@pid", personId));

            return rows > 0;
        }

        var updated = await _db.ExecuteAsync(@"
        UPDATE Persons
           SET First_Name = @fn,
               Last_Name  = @ln,
               WorkEmail  = @em,
               Cell_No    = @cn
         WHERE Person_ID = @pid",
            new SqlParameter("@fn", req.FirstName.Trim()),
            new SqlParameter("@ln", req.LastName.Trim()),
            new SqlParameter("@em", (object?)req.WorkEmail?.Trim() ?? DBNull.Value),
            new SqlParameter("@cn", (object?)req.CellNo?.Trim() ?? DBNull.Value),
            new SqlParameter("@pid", personId));

        return updated > 0;
    }

    public async Task<bool> UpdateUserByAdminAsync(int personId, UpdateUserByAdminRequest req)
    {
        var rows = await _db.ExecuteAsync(@"
        UPDATE Persons
           SET First_Name = @fn,
               Last_Name  = @ln,
               WorkEmail  = @em,
               Cell_No    = @cn
         WHERE Person_ID = @pid;",
            new SqlParameter("@fn", req.FirstName.Trim()),
            new SqlParameter("@ln", req.LastName.Trim()),
            new SqlParameter("@em", (object?)req.WorkEmail?.Trim() ?? DBNull.Value),
            new SqlParameter("@cn", (object?)req.CellNo?.Trim() ?? DBNull.Value),
            new SqlParameter("@pid", personId));

        return rows > 0;
    }
    public async Task<bool> WorkEmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var count = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM Persons WHERE WorkEmail = @em",
            new SqlParameter("@em", email.Trim()));

        return Convert.ToInt32(count) > 0;
    }

    public async Task<bool> CellNoExistsAsync(string cellNo)
    {
        if (string.IsNullOrWhiteSpace(cellNo)) return false;

        var count = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM Persons WHERE Cell_No = @cn",
            new SqlParameter("@cn", cellNo.Trim()));

        return Convert.ToInt32(count) > 0;
    }
}