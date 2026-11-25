namespace ModularAgenda.Core.Services.Reminders;

/// <summary>
/// Observer deseninin "dinleyen" tarafı. Hatırlatıcının zamanı geldiğinde haber almak
/// isteyen her sınıf bu arayüzü uygular ve ReminderService'e abone olur. Asıl dinleyen,
/// ekranlardaki uyarı penceresi olacak.
/// </summary>
public interface IReminderObserver
{
    void OnReminderDue(ReminderAlert alert);
}
