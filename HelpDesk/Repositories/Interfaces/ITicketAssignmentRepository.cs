namespace HelpDesk.Repositories.Interfaces;

public interface ITicketAssignmentRepository
{
    Task<int> CreateAsync(int adminId, int municipalityId, int ussId,
                          int ticketId, string instruction, DateTime deadline);
    Task<bool> ExistsForTicketAsync(int ticketId);
}