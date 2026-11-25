using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data.Repositories;

public class EfNoteRepository : EfRepository<Note>, INoteRepository
{
    public EfNoteRepository(AppDbContext context) : base(context)
    {
    }

    // TODO: Note'a TargetId eklenince doldurulacak
    public Task<IReadOnlyList<Note>> GetByTargetAsync(Guid targetId)
    {
        return Task.FromResult<IReadOnlyList<Note>>(Array.Empty<Note>());
    }

    public async Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Notes
            .Where(n => n.Date >= start && n.Date <= end)
            .ToListAsync();
    }
}
