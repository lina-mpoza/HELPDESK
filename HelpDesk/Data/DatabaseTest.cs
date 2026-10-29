using Microsoft.Data.SqlClient;

namespace HelpDesk.Data;

public static class DatabaseTest
{
    public static async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Statuses", conn);
            var count = await cmd.ExecuteScalarAsync();

            Console.WriteLine("Connected to MunicipalHelpdesk");
            Console.WriteLine($"   Statuses table has {count} rows.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Database connection FAILED");
            Console.WriteLine($"   {ex.Message}");
            return false;
        }
    }
}