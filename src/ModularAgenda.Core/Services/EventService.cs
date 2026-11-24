using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services;

/// <summary>Etkinliklerin servisi.</summary>
public class EventService : CrudService<CalendarEvent>
{
    private readonly LinkedItemsCleaner _linkedItems;

    public EventService(ICalendarEventRepository events, LinkedItemsCleaner linkedItems) : base(events)
    {
        ArgumentNullException.ThrowIfNull(linkedItems);
        _linkedItems = linkedItems;
    }

    /// <summary>Etkinliği siler; ona bağlı notları ve hatırlatıcıları da siler (K2.6).</summary>
    public override async Task DeleteAsync(CalendarEvent calendarEvent)
    {
        await base.DeleteAsync(calendarEvent);
        await _linkedItems.DeleteNotesOfAsync(calendarEvent);
        await _linkedItems.DeleteRemindersOfAsync(calendarEvent);
    }

    // CalendarEvent başlangıç ve bitiş saatlerini kazanınca eklenecek: FindConflictsAsync.
    // Kural hazır: iki etkinlik, a.Start < b.End && b.Start < a.End ise çakışır.
}
