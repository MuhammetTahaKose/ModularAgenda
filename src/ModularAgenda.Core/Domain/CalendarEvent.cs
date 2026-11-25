using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class CalendarEvent : AgendaItem, ICalendarItem, INoteable, IRemindable
{
    public string Title { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public DateOnly? CalendarDate => StartDate;

    public override string Summary => Title;
}

