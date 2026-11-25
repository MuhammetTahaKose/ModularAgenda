using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class TodoItem : AgendaItem, ICalendarItem, INoteable, IRemindable
{
    public string Title { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }

    public DateOnly? CalendarDate => DueDate;

    public override string Summary => Title;
}

