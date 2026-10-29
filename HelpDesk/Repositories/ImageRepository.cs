using HelpDesk.Data;
using HelpDesk.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace HelpDesk.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly DbHelper _db;
    public ImageRepository(DbHelper db) => _db = db;

    public async Task<int> CreateAsync(ImageRecord img)
    {
        var id = await _db.ExecuteScalarAsync(@"
            INSERT INTO Images (Blob_Content, File_Name, Mime_Type)
            OUTPUT INSERTED.Image_ID
            VALUES (@blob, @name, @mime);",
            new SqlParameter("@blob", (object?)img.Blob_Content ?? DBNull.Value),
            new SqlParameter("@name", (object?)img.File_Name ?? DBNull.Value),
            new SqlParameter("@mime", (object?)img.Mime_Type ?? DBNull.Value));

        return Convert.ToInt32(id);
    }

    public async Task<ImageRecord?> GetByIdAsync(int imageId)
    {
        var table = await _db.QueryAsync(
            "SELECT Image_ID, Blob_Content, File_Name, Mime_Type FROM Images WHERE Image_ID = @id",
            new SqlParameter("@id", imageId));

        if (table.Rows.Count == 0) return null;
        var r = table.Rows[0];

        return new ImageRecord
        {
            Image_ID = (int)r["Image_ID"],
            Blob_Content = r["Blob_Content"] as byte[],
            File_Name = r["File_Name"] as string,
            Mime_Type = r["Mime_Type"] as string
        };
    }
}