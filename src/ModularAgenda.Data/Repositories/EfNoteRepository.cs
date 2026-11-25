using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data.Repositories;

public class EfNoteRepository : EfRepository<Note>, INoteRepository
{
    public EfNoteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Note>> GetByTargetAsync(Guid targetId)
    {
        return await _context.Notes
            .Where(n => n.TargetId == targetId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Notes
            .Where(n => n.Date >= start && n.Date <= end)
            .ToListAsync();
    }
}
