namespace HelpDesk.Repositories.Interfaces;

public interface IUSSTicketTaskRepository
{
    Task<int> CreateAsync(int ticketId, int ussId, int statusId, string note, DateTime? dateSolved);
}