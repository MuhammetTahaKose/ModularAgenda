using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data.Repositories;

public class EfTodoRepository : EfRepository<TodoItem>, ITodoRepository
{
    public EfTodoRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TodoItem>> GetByDueDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.TodoItems
            .Where(t => t.DueDate >= start && t.DueDate <= end)
            .ToListAsync();
    }
}
