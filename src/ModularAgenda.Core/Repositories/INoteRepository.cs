using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Repositories;

public interface INoteRepository : IRepository<Note>
{
    Task<IReadOnlyList<Note>> GetByTargetAsync(Guid targetId);
    Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateOnly start, DateOnly end);
}
