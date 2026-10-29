using HelpDesk.DTOs.Requests;
using HelpDesk.DTOs.Responses;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services.Interfaces;

namespace HelpDesk.Services;

public class AdminUsersService : IAdminUsersService
{
    private readonly ILookupRepository _lookups;
    private readonly ITechnicianAdminRepository _technicians;
    private readonly IEmailService _email;

    public AdminUsersService(
        ILookupRepository lookups,
        ITechnicianAdminRepository technicians,
        IEmailService email)
    {
        _lookups = lookups;
        _technicians = technicians;
        _email = email;
    }

    public async Task<ApiResponse> GetLookupsAsync(int adminPersonId)
    {
        var municipality = await _lookups.GetAdminMunicipalityAsync(adminPersonId);
        if (municipality == null)
            return ApiResponse.Fail(
                "Your admin account is not linked to a municipality. " +
                "Please set your Staff.MDID in the database.");

        var result = new LookupListsResponse
        {
            JobTitles = await _lookups.GetJobTitlesAsync(),
            EmploymentStatuses = await _lookups.GetEmploymentStatusesAsync(),
            Municipalities = new List<LookupItem> { municipality },
            Departments = await _lookups.GetDepartmentsForMunicipalityAsync(municipality.Id)
        };

        return ApiResponse.Ok("Lookups loaded.", result);
    }

    public async Task<ApiResponse> GetDepartmentsForMunicipalityAsync(int municipalityId)
    {
        var list = await _lookups.GetDepartmentsForMunicipalityAsync(municipalityId);
        return ApiResponse.Ok($"{list.Count} department(s).", list);
    }

    public async Task<ApiResponse> CreateTechnicianAsync(CreateTechnicianRequest req)
    {
        // alidation
        if (string.IsNullOrWhiteSpace(req.FirstName) ||
            string.IsNullOrWhiteSpace(req.LastName) ||
            string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password) ||
            string.IsNullOrWhiteSpace(req.StaffNo))
            return ApiResponse.Fail("Please fill in all required fields.");

        if (req.Password.Length < 6)
            return ApiResponse.Fail("Password must be at least 6 characters.");

        if (string.IsNullOrWhiteSpace(req.WorkEmail))
            return ApiResponse.Fail("Work email is required — credentials are sent to it.");

        if (req.JobId <= 0) return ApiResponse.Fail("Please choose a job title.");
        if (req.EStatusId <= 0) return ApiResponse.Fail("Please choose an employment status.");
        if (req.MID <= 0) return ApiResponse.Fail("Please choose a municipality.");
        if (req.DepId <= 0) return ApiResponse.Fail("Please choose a department.");

        if (await _technicians.UsernameExistsAsync(req.Username.Trim()))
            return ApiResponse.Fail("That username is already taken.");

        if (await _technicians.StaffNoExistsAsync(req.StaffNo.Trim()))
            return ApiResponse.Fail("That staff number is already in use.");
        if (await _technicians.WorkEmailExistsAsync(req.WorkEmail))
            return ApiResponse.Fail("That email address is already registered.");

        // cell number uniqueness
        if (!string.IsNullOrWhiteSpace(req.CellNo) &&
            await _technicians.CellNoExistsAsync(req.CellNo))
            return ApiResponse.Fail("That cell number is already registered.");

        // Create
        var (ok, message, personId) = await _technicians.CreateTechnicianAsync(req);

        if (!ok)
            return ApiResponse.Fail("Could not create technician: " + message);

        // Send credentials email
        string emailStatus;
        try
        {
            await _email.SendTechnicianCredentialsAsync(
                req.WorkEmail.Trim(),
                $"{req.FirstName.Trim()} {req.LastName.Trim()}",
                req.Username.Trim(),
                req.Password,
                req.StaffNo.Trim(),
                string.IsNullOrWhiteSpace(req.OfficeLocation) ? "—" : req.OfficeLocation.Trim()
            );
            emailStatus = "Credentials emailed successfully.";
        }
        catch (Exception ex)
        {
            emailStatus = "Account created, but the credentials email could NOT be sent: " + ex.Message;
            Console.WriteLine($"Email send failed: {ex}");
        }

        return ApiResponse.Ok(
            "Technician created successfully. " + emailStatus,
            new { personId, email = req.WorkEmail });
    }

    public async Task<ApiResponse> GetAllUsersAsync()
    {
        var list = await _technicians.GetAllUsersAsync();
        return ApiResponse.Ok($"{list.Count} user(s).", list);
    }

    public async Task<ApiResponse> GetProfileAsync(int personId)
    {
        var p = await _technicians.GetProfileAsync(personId);
        if (p == null) return ApiResponse.Fail("Profile not found.");
        return ApiResponse.Ok("Profile loaded.", p);
    }

    public async Task<ApiResponse> UpdateProfileAsync(int personId, UpdateProfileRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) ||
            string.IsNullOrWhiteSpace(req.LastName))
            return ApiResponse.Fail("First name and last name are required.");

        if (string.IsNullOrWhiteSpace(req.WorkEmail))
            return ApiResponse.Fail("Work email is required.");

        if (!string.IsNullOrWhiteSpace(req.NewPassword) && req.NewPassword.Length < 6)
            return ApiResponse.Fail("New password must be at least 6 characters.");

        var ok = await _technicians.UpdateProfileAsync(personId, req);
        if (!ok) return ApiResponse.Fail("Could not update profile.");

        // Return the updated values so the frontend can refresh the session
        var refreshed = await _technicians.GetProfileAsync(personId);
        return ApiResponse.Ok("Profile updated successfully.", refreshed);
    }

    public async Task<ApiResponse> UpdateUserByAdminAsync(int personId, UpdateUserByAdminRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) ||
            string.IsNullOrWhiteSpace(req.LastName))
            return ApiResponse.Fail("First name and last name are required.");

        if (string.IsNullOrWhiteSpace(req.WorkEmail))
            return ApiResponse.Fail("Work email is required.");

        var ok = await _technicians.UpdateUserByAdminAsync(personId, req);
        if (!ok) return ApiResponse.Fail("Could not update user.");

        // Return the refreshed values so the UI can update immediately
        var refreshed = await _technicians.GetProfileAsync(personId);
        return ApiResponse.Ok("User updated successfully.", refreshed);
    }
}