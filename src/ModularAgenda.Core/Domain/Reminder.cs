namespace ModularAgenda.Core.Domain;

public abstract class Reminder : AgendaItem
{
    public Guid TargetId { get; private set; }                       // bağlı olduğu kaydın Id'si

    protected Reminder(Guid targetId)
    {
        TargetId = targetId;
    }

    public abstract DateTime? GetNextTriggerTime(DateTime nowUtc);   // bir daha çalmayacaksa null
}
