using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class Habit : AgendaItem, IRemindable
{
    // TODO(Kadir):
    public override string Summary => throw new NotImplementedException("Kadir dolduracak");
}
