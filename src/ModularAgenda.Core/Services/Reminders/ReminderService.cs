using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services.Reminders;

/// <summary>
/// Hatırlatıcıların servisi ve Observer deseninin "haber veren" (subject) tarafı.
/// Uygulama bir zamanlayıcıyla CheckDueRemindersAsync'i düzenli çağırır; zamanı gelen
/// hatırlatıcılar abone olan tüm gözlemcilere bildirilir. Servis gözlemcilerin ne
/// olduğunu bilmez (pencere mi, ses mi); sadece arayüzü çağırır.
/// </summary>
public class ReminderService : CrudService<Reminder>
{
    private readonly IReminderRepository _reminders;
    private readonly ITodoRepository _todos;
    private readonly ICalendarEventRepository _events;
    private readonly IRepository<Habit> _habits;
    private readonly List<IReminderObserver> _observers = new();
    private DateTime _lastCheckUtc;

    /// <param name="checkFromUtc">
    /// Kontrolün başlayacağı an; verilmezse şimdi. Uygulama kapalıyken zamanı geçen
    /// hatırlatıcılar açılışta gösterilmez.
    /// </param>
    public ReminderService(
        IReminderRepository reminders,
        ITodoRepository todos,
        ICalendarEventRepository events,
        IRepository<Habit> habits,
        DateTime? checkFromUtc = null) : base(reminders)
    {
        ArgumentNullException.ThrowIfNull(todos);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(habits);
        _reminders = reminders;
        _todos = todos;
        _events = events;
        _habits = habits;
        _lastCheckUtc = checkFromUtc ?? DateTime.UtcNow;
    }

    public void Subscribe(IReminderObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IReminderObserver observer) => _observers.Remove(observer);

    /// <summary>Bir göreve, etkinliğe ya da alışkanlığa bağlı hatırlatıcılar.</summary>
    public async Task<IReadOnlyList<Reminder>> GetForTargetAsync(IRemindable target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return await _reminders.GetByTargetAsync(target.Id);
    }

    /// <summary>
    /// Son kontrolden bu yana çalma zamanı gelen hatırlatıcıları bulur ve gözlemcilere haber
    /// verir. Hatırlatıcının tek seferlik mi günlük mü olduğuna bakmaz; ikisinde de
    /// GetNextTriggerTime'ı çağırır ve her tür kendi yöntemiyle cevap verir (çok biçimlilik).
    /// </summary>
    /// <param name="nowUtc">Şu anki zaman (UTC); testlerde sabit bir an verilebilsin diye dışarıdan alınır.</param>
    /// <returns>Gösterilen uyarı sayısı.</returns>
    public async Task<int> CheckDueRemindersAsync(DateTime nowUtc)
    {
        DateTime since = _lastCheckUtc;
        if (nowUtc <= since)
            return 0; // Saat geri alınmışsa aynı hatırlatıcı iki kez çalmasın.
        _lastCheckUtc = nowUtc;

        var alerts = new List<ReminderAlert>();
        foreach (Reminder reminder in await _reminders.GetAllAsync())
        {
            // Son kontrolden sonraki ilk çalma zamanı şimdiye kadar geldiyse hatırlatıcı çalar.
            DateTime? next = reminder.GetNextTriggerTime(since);
            if (next is null || next <= since || next > nowUtc)
                continue;

            AgendaItem? target = await FindTargetAsync(reminder.TargetId);
            if (target is null)
                continue; // Bağlı kayıt silinmiş; hatırlatıcı da sessiz kalır.

            // Hatırlatıcı bağlı olduğu kaydın adını bilmez; birleşik metni servis kurar (bölüm 4.1).
            string message = $"{reminder.Summary}, {target.Summary}";
            alerts.Add(new ReminderAlert(reminder, target, message, next.Value));
        }

        foreach (ReminderAlert alert in alerts.OrderBy(a => a.TriggeredAtUtc))
            NotifyObservers(alert);

        return alerts.Count;
    }

    /// <summary>Hatırlatıcı yalnızca görev, etkinlik ya da alışkanlığa bağlanabilir (IRemindable).</summary>
    private async Task<AgendaItem?> FindTargetAsync(Guid targetId)
    {
        AgendaItem? target = await _todos.GetByIdAsync(targetId);
        target ??= await _events.GetByIdAsync(targetId);
        target ??= await _habits.GetByIdAsync(targetId);
        return target;
    }

    private void NotifyObservers(ReminderAlert alert)
    {
        // Listenin kopyası üzerinde dönülür: bir gözlemci bildirim sırasında abonelikten
        // çıkarsa asıl liste değişir ve foreach hata verirdi.
        foreach (IReminderObserver observer in _observers.ToList())
            observer.OnReminderDue(alert);
    }
}
