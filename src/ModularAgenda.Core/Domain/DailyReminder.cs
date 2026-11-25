namespace ModularAgenda.Core.Domain;

public class DailyReminder : Reminder
{
    // TODO(Kadir): Reminder'ın tek constructor'ı targetId istediği için derleme açısından zorunlu.
    public DailyReminder(Habit target, TimeOnly time) : base(target.Id)
    {
        throw new NotImplementedException("Kadir dolduracak");
    }

    // EF Core için
    private DailyReminder() { }

    // TODO(Kadir):
    public override string Summary => throw new NotImplementedException("Kadir dolduracak");

    // TODO(Kadir):
    public override DateTime? GetNextTriggerTime(DateTime nowUtc)
    {
        throw new NotImplementedException("Kadir dolduracak");
    }
}
