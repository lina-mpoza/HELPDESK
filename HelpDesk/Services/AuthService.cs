using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;
using HelpDesk.Models;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class AuthService : IAuthService
{
    private readonly IPersonRepository _persons;
    private readonly IStaffRepository _staff;
    private readonly IGeneralStaffRepository _generalStaff;

    public AuthService(
        IPersonRepository persons,
        IStaffRepository staff,
        IGeneralStaffRepository generalStaff)
    {
        _persons = persons;
        _staff = staff;
        _generalStaff = generalStaff;
    }

    public async Task<ApiResponse> RegisterAsync(RegisterRequest req)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(req.FirstName) ||
            string.IsNullOrWhiteSpace(req.LastName) ||
            string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password))
            return ApiResponse.Fail("First name, last name, username and password are required.");

        if (req.Password.Length < 6)
            return ApiResponse.Fail("Password must be at least 6 characters.");

        if (await _persons.UsernameExistsAsync(req.Username))
            return ApiResponse.Fail("That username is already taken.");

        if (await _persons.UsernameExistsAsync(req.Username))
            return ApiResponse.Fail("That username is already taken.");

        if (!string.IsNullOrWhiteSpace(req.WorkEmail) &&
            await _persons.WorkEmailExistsAsync(req.WorkEmail))
            return ApiResponse.Fail("That email address is already registered.");

        if (!string.IsNullOrWhiteSpace(req.CellNo) &&
            await _persons.CellNoExistsAsync(req.CellNo))
            return ApiResponse.Fail("That cell number is already registered.");

        // Create Person
        var personId = await _persons.CreateAsync(new Person
        {
            First_Name = req.FirstName.Trim(),
            Last_Name = req.LastName.Trim(),
            WorkEmail = req.WorkEmail?.Trim(),
            Cell_No = req.CellNo?.Trim(),
            Username = req.Username.Trim(),
            Password = req.Password
        });

        // Create Staff row
        // Every signup gets a Staff row
        var staffId = await _staff.CreateAsync(personId,
            string.IsNullOrWhiteSpace(req.StaffNo) ? null : req.StaffNo.Trim());

        // Link as General Staff
        await _generalStaff.CreateAsync(staffId);

        return ApiResponse.Ok("Account created successfully.", new
        {
            personId,
            staffId,
            role = "GeneralStaff"
        });
    }
    public async Task<ApiResponse> LoginAsync(LoginRequest req)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password))
            return ApiResponse.Fail("Username and password are required.");

        // Look up person
        var person = await _persons.GetByUsernameAsync(req.Username.Trim());
        if (person == null)
            return ApiResponse.Fail("Invalid username or password.");

        // Password check
        if (!string.Equals(person.Password, req.Password, StringComparison.Ordinal))
            return ApiResponse.Fail("Invalid username or password.");

        // Resolve staff + role
        var staffId = await _staff.GetStaffIdByPersonIdAsync(person.Person_ID);
        if (staffId == null)
            return ApiResponse.Fail("No staff record is linked to this account.");

        var role = await _staff.GetRoleByStaffIdAsync(staffId.Value);

        // Success
        return ApiResponse.Ok("Login successful.", new LoginResponse
        {
            PersonId = person.Person_ID,
            StaffId = staffId.Value,
            Username = person.Username,
            FirstName = person.First_Name ?? "",
            LastName = person.Last_Name ?? "",
            Role = role
        });
    }
}