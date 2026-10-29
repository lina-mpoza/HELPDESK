using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly DbHelper _db;
    public PersonRepository(DbHelper db) => _db = db;

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var count = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM Persons WHERE Username = @u",
            new SqlParameter("@u", username));

        return Convert.ToInt32(count) > 0;
    }

    public async Task<int> CreateAsync(Person p)
    {
        var id = await _db.ExecuteScalarAsync(@"
            INSERT INTO Persons
                (First_Name, Last_Name, WorkEmail, Cell_No, Username, Password)
            OUTPUT INSERTED.Person_ID
            VALUES (@fn, @ln, @em, @cn, @un, @pw);",
            new SqlParameter("@fn", p.First_Name ?? ""),
            new SqlParameter("@ln", p.Last_Name ?? ""),
            new SqlParameter("@em", (object?)p.WorkEmail ?? DBNull.Value),
            new SqlParameter("@cn", (object?)p.Cell_No ?? DBNull.Value),
            new SqlParameter("@un", p.Username),
            new SqlParameter("@pw", (object?)p.Password ?? DBNull.Value));

        return Convert.ToInt32(id);
    }

    public async Task<Person?> GetByUsernameAsync(string username)
    {
        var table = await _db.QueryAsync(
            "SELECT * FROM Persons WHERE Username = @u",
            new Microsoft.Data.SqlClient.SqlParameter("@u", username));

        if (table.Rows.Count == 0) return null;

        var r = table.Rows[0];
        return new Person
        {
            Person_ID = (int)r["Person_ID"],
            First_Name = r["First_Name"] as string,
            Last_Name = r["Last_Name"] as string,
            WorkEmail = r["WorkEmail"] as string,
            Cell_No = r["Cell_No"] as string,
            Username = (string)r["Username"],
            Password = r["Password"] as string,
            Registered_At = r["Registered_At"] as DateTime?
        };
    }
    public async Task<Person?> GetByEmailAsync(string email)
    {
        var table = await _db.QueryAsync(
            "SELECT * FROM Persons WHERE WorkEmail = @em",
            new Microsoft.Data.SqlClient.SqlParameter("@em", email));

        if (table.Rows.Count == 0) return null;
        var r = table.Rows[0];

        return new Person
        {
            Person_ID = (int)r["Person_ID"],
            First_Name = r["First_Name"] as string,
            Last_Name = r["Last_Name"] as string,
            WorkEmail = r["WorkEmail"] as string,
            Cell_No = r["Cell_No"] as string,
            Username = (string)r["Username"],
            Password = r["Password"] as string,
            Registered_At = r["Registered_At"] as DateTime?
        };
    }

    public async Task<bool> SaveResetOtpAsync(int personId, string otp, DateTime expiry)
    {
        var rows = await _db.ExecuteAsync(@"
        UPDATE Persons
           SET Reset_Otp    = @otp,
               Reset_Expiry = @exp
         WHERE Person_ID = @pid;",
            new Microsoft.Data.SqlClient.SqlParameter("@otp", otp),
            new Microsoft.Data.SqlClient.SqlParameter("@exp", expiry),
            new Microsoft.Data.SqlClient.SqlParameter("@pid", personId));

        return rows > 0;
    }

    public async Task<bool> ResetPasswordAsync(int personId, string newPassword)
    {
        var rows = await _db.ExecuteAsync(@"
        UPDATE Persons
           SET Password     = @pw,
               Reset_Otp    = NULL,
               Reset_Expiry = NULL
         WHERE Person_ID = @pid;",
            new Microsoft.Data.SqlClient.SqlParameter("@pw", newPassword),
            new Microsoft.Data.SqlClient.SqlParameter("@pid", personId));

        return rows > 0;
    }

    public async Task<(bool ok, string message)> VerifyOtpAndResetPasswordAsync(
        string email, string otp, string newPassword)
    {
        var table = await _db.QueryAsync(@"
        SELECT Person_ID, Reset_Otp, Reset_Expiry
        FROM   Persons
        WHERE  WorkEmail = @em",
            new Microsoft.Data.SqlClient.SqlParameter("@em", email));

        if (table.Rows.Count == 0)
            return (false, "Invalid OTP or email.");

        var r = table.Rows[0];
        var storedOtp = r["Reset_Otp"] as string;
        var storedExpiry = r["Reset_Expiry"] as DateTime?;

        if (string.IsNullOrEmpty(storedOtp) || storedExpiry == null)
            return (false, "No active password reset request. Please request a new OTP.");

        if (storedExpiry.Value < DateTime.UtcNow)
            return (false, "The OTP has expired. Please request a new one.");

        if (!string.Equals(storedOtp, otp, StringComparison.Ordinal))
            return (false, "Invalid OTP. Please try again.");

        var personId = (int)r["Person_ID"];

        var rows = await _db.ExecuteAsync(@"
        UPDATE Persons
           SET Password     = @pw,
               Reset_Otp    = NULL,
               Reset_Expiry = NULL
         WHERE Person_ID = @pid;",
            new Microsoft.Data.SqlClient.SqlParameter("@pw", newPassword),
            new Microsoft.Data.SqlClient.SqlParameter("@pid", personId));

        return rows > 0
            ? (true, "Password reset successful.")
            : (false, "Could not reset password.");
    }
    public async Task<bool> WorkEmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var count = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM Persons WHERE WorkEmail = @em",
            new Microsoft.Data.SqlClient.SqlParameter("@em", email.Trim()));

        return Convert.ToInt32(count) > 0;
    }

    public async Task<bool> CellNoExistsAsync(string cellNo)
    {
        if (string.IsNullOrWhiteSpace(cellNo)) return false;

        var count = await _db.ExecuteScalarAsync(
            "SELECT COUNT(1) FROM Persons WHERE Cell_No = @cn",
            new Microsoft.Data.SqlClient.SqlParameter("@cn", cellNo.Trim()));

        return Convert.ToInt32(count) > 0;
    }
}