using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Repositories;

public interface IReminderRepository : IRepository<Reminder>
{
    Task<IReadOnlyList<Reminder>> GetByTargetAsync(Guid targetId);
}
