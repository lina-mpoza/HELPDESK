namespace HelpDesk.DTOs.Responses;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }

    public static ApiResponse Ok(string message = "Success", object? data = null)
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse Fail(string message)
        => new() { Success = false, Message = message };
}