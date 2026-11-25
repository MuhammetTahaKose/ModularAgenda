using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data.Repositories;

public class EfCalendarEventRepository : EfRepository<CalendarEvent>, ICalendarEventRepository
{
    public EfCalendarEventRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CalendarEvent>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.CalendarEvents
            .Where(e => e.StartDate >= start && e.StartDate <= end)
            .ToListAsync();
    }
}
