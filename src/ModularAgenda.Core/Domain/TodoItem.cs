using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class TodoItem : AgendaItem, ICalendarItem, INoteable, IRemindable
{
    // TODO(Kadir):
    public DateOnly? CalendarDate => throw new NotImplementedException("Kadir dolduracak");

    // TODO(Kadir):
    public override string Summary => throw new NotImplementedException("Kadir dolduracak");
}
