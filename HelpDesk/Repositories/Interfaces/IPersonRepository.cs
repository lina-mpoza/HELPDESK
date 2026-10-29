using HelpDesk.Models;

namespace HelpDesk.Repositories.Interfaces;

public interface IPersonRepository
{
    Task<bool> UsernameExistsAsync(string username);
    Task<int> CreateAsync(Person person);
    Task<Person?> GetByUsernameAsync(string username);
    Task<Person?> GetByEmailAsync(string email);
    Task<bool> SaveResetOtpAsync(int personId, string otp, DateTime expiry);
    Task<bool> ResetPasswordAsync(int personId, string newPassword);
    Task<(bool ok, string message)> VerifyOtpAndResetPasswordAsync(
        string email, string otp, string newPassword);
    Task<bool> WorkEmailExistsAsync(string email);
    Task<bool> CellNoExistsAsync(string cellNo);
}