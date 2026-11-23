using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class Note : AgendaItem, ICalendarItem
{
    // TODO(Kadir):
    public DateOnly? CalendarDate => throw new NotImplementedException("Kadir dolduracak");

    // TODO(Kadir):
    public override string Summary => throw new NotImplementedException("Kadir dolduracak");
}
