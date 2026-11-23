using ModularAgenda.Core.Services.Reminders;

namespace ModularAgenda.Tests.Services.Reminders;

/// <summary>Sahte gözlemci: kendisine gelen uyarıları bir listeye yazar, test o listeye bakar.</summary>
public class RecordingObserver : IReminderObserver
{
    public List<ReminderAlert> Received { get; } = new();

    public void OnReminderDue(ReminderAlert alert) => Received.Add(alert);
}
