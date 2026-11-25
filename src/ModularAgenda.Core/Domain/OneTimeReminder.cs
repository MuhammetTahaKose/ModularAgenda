using ModularAgenda.Core.Abstractions;

namespace ModularAgenda.Core.Domain;

public class OneTimeReminder : Reminder
{
    // TODO(Kadir): Reminder'ın tek constructor'ı targetId istediği için derleme açısından zorunlu.
    public OneTimeReminder(IRemindable target, DateTime timeUtc) : base(target.Id)
    {
        throw new NotImplementedException("Kadir dolduracak");
    }

    // EF Core için
    private OneTimeReminder() { }

    // TODO(Kadir):
    public override string Summary => throw new NotImplementedException("Kadir dolduracak");

    // TODO(Kadir):
    public override DateTime? GetNextTriggerTime(DateTime nowUtc)
    {
        throw new NotImplementedException("Kadir dolduracak");
    }
}
