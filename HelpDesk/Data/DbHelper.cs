using Microsoft.Data.SqlClient;
using System.Data;

namespace HelpDesk.Data;

public class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<int> ExecuteAsync(string sql, params SqlParameter[] parameters)
    {
        using var conn = CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
    {
        using var conn = CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
        await conn.OpenAsync();
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<DataTable> QueryAsync(string sql, params SqlParameter[] parameters)
    {
        using var conn = CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
        using var adapter = new SqlDataAdapter(cmd);
        var table = new DataTable();
        await Task.Run(() => adapter.Fill(table));
        return table;
    }
}