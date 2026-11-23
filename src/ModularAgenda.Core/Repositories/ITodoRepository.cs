using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Repositories;

public interface ITodoRepository : IRepository<TodoItem>
{
    Task<IReadOnlyList<TodoItem>> GetByDueDateRangeAsync(DateOnly start, DateOnly end);
}
