using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class Note : AgendaItem, ICalendarItem
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? TargetId { get; set; }
    public DateOnly? Date { get; set; }

    public DateOnly? CalendarDate => Date;

    public override string Summary => Title;
}

