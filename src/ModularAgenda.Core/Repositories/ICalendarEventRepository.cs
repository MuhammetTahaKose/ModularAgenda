using ModularAgenda.Core.Domain;

namespace ModularAgenda.Core.Repositories;

public interface ICalendarEventRepository : IRepository<CalendarEvent>
{
    Task<IReadOnlyList<CalendarEvent>> GetByDateRangeAsync(DateOnly start, DateOnly end);
}
