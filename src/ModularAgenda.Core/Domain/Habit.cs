using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class Habit : AgendaItem, IRemindable
{
    public string Name { get; set; } = string.Empty;

    public override string Summary => Name;
}

