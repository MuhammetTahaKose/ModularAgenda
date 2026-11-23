using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;

namespace ModularAgenda.Tests.Fakes;

// =============================================================================
// GEÇİCİ: Domain sınıflarının Summary ve CalendarDate üyeleri şu an
// NotImplementedException fırlatıyor (Kadir dolduracak). Servis testleri onu
// beklemesin diye bu alt sınıflar Summary'yi ezer ve ICalendarItem.CalendarDate'i
// yeniden uygular. Gerçek sınıflar dolunca bu dosya silinir; testler gerçek
// constructor'larla nesne oluşturur.
// =============================================================================

internal sealed class TestTodo : TodoItem, ICalendarItem
{
    private readonly string _summary;
    private readonly DateOnly? _dueDate;

    public TestTodo(string summary, DateOnly? dueDate = null)
    {
        _summary = summary;
        _dueDate = dueDate;
    }

    public override string Summary => _summary;
    DateOnly? ICalendarItem.CalendarDate => _dueDate;
}

internal sealed class TestEvent : CalendarEvent, ICalendarItem
{
    private readonly string _summary;
    private readonly DateOnly _startDate;

    public TestEvent(string summary, DateOnly startDate)
    {
        _summary = summary;
        _startDate = startDate;
    }

    public override string Summary => _summary;
    DateOnly? ICalendarItem.CalendarDate => _startDate;
}

internal sealed class TestNote : Note, ICalendarItem
{
    private readonly string _summary;
    private readonly DateOnly? _day;

    /// <param name="day">Güne bağlı notsa o gün; göreve ya da etkinliğe bağlıysa null.</param>
    public TestNote(string summary, DateOnly? day = null)
    {
        _summary = summary;
        _day = day;
    }

    public override string Summary => _summary;
    DateOnly? ICalendarItem.CalendarDate => _day;
}

internal sealed class TestHabit : Habit
{
    private readonly string _summary;

    public TestHabit(string summary)
    {
        _summary = summary;
    }

    public override string Summary => _summary;
}

/// <summary>
/// Tek seferlik ya da günlük hatırlatıcı gibi davranan test hatırlatıcısı. Gerçek
/// OneTimeReminder ve DailyReminder gibi GetNextTriggerTime'ı kendi yöntemiyle uygular.
/// </summary>
internal sealed class TestReminder : Reminder
{
    private readonly string _summary;
    private readonly Func<DateTime, DateTime?> _nextTrigger;

    private TestReminder(IRemindable target, string summary, Func<DateTime, DateTime?> nextTrigger)
        : base(target.Id)
    {
        _summary = summary;
        _nextTrigger = nextTrigger;
    }

    public override string Summary => _summary;

    public override DateTime? GetNextTriggerTime(DateTime nowUtc) => _nextTrigger(nowUtc);

    /// <summary>Verilen anda bir kez çalar; o an geçtiyse bir daha çalmaz (null).</summary>
    public static TestReminder Once(IRemindable target, string summary, DateTime atUtc) =>
        new(target, summary, after => atUtc > after ? atUtc : null);

    /// <summary>
    /// Once gibi, ama "verilen andan sonra" yerine "verilen anda ya da sonra" der:
    /// az önce çalan zamanı bir sonraki kontrolde yine döndürür.
    /// </summary>
    public static TestReminder OnceInclusive(IRemindable target, string summary, DateTime atUtc) =>
        new(target, summary, after => atUtc >= after ? atUtc : null);

    /// <summary>Her gün aynı saatte çalar.</summary>
    public static TestReminder Daily(IRemindable target, string summary, TimeSpan timeOfDayUtc) =>
        new(target, summary, after =>
        {
            DateTime today = after.Date + timeOfDayUtc;
            return today > after ? today : today.AddDays(1);
        });
}
