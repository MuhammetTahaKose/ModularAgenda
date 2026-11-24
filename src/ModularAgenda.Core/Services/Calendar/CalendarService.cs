using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services.Calendar;

/// <summary>
/// Takvim sayfası (ana ekran) için veri hazırlar. Izgara hazır bileşen kullanılmadan
/// koddan üretilir (Karar K1.3); bir ay açılırken üç tarih aralığı sorgusu çağrılır ve
/// sonuçlar ICalendarItem olarak birleştirilir (bölüm 5).
/// </summary>
public class CalendarService
{
    /// <summary>Izgara her zaman 6 hafta x 7 gün = 42 kutu; yüksekliği aydan aya değişmez.</summary>
    public const int GridCellCount = 42;

    private readonly ICalendarEventRepository _events;
    private readonly ITodoRepository _todos;
    private readonly INoteRepository _notes;

    public CalendarService(ICalendarEventRepository events, ITodoRepository todos, INoteRepository notes)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(todos);
        ArgumentNullException.ThrowIfNull(notes);
        _events = events;
        _todos = todos;
        _notes = notes;
    }

    /// <summary>Ekranın kullanacağı hâli: "bugün" bilgisayarın tarihidir.</summary>
    public Task<IReadOnlyList<CalendarDay>> GetMonthGridAsync(int year, int month) =>
        GetMonthGridAsync(year, month, DateOnly.FromDateTime(DateTime.Today));

    /// <summary>
    /// Ayın ızgarasını pazartesiden başlayarak 42 gün olarak döndürür. Ayın 1'inden önceki
    /// ve son gününden sonraki kutular komşu aylardan gelir (IsInCurrentMonth = false).
    /// </summary>
    /// <param name="today">Testlerde "bugün"ü sabitlemek için dışarıdan verilebilir.</param>
    public async Task<IReadOnlyList<CalendarDay>> GetMonthGridAsync(int year, int month, DateOnly today)
    {
        var firstOfMonth = new DateOnly(year, month, 1);
        DateOnly gridStart = firstOfMonth.AddDays(-DaysSinceMonday(firstOfMonth));
        DateOnly gridEnd = gridStart.AddDays(GridCellCount - 1);

        IReadOnlyList<ICalendarItem> items = await GetItemsBetweenAsync(gridStart, gridEnd);

        var cells = new List<CalendarDay>(GridCellCount);
        for (int i = 0; i < GridCellCount; i++)
        {
            DateOnly date = gridStart.AddDays(i);
            cells.Add(new CalendarDay(
                Date: date,
                IsInCurrentMonth: date.Year == year && date.Month == month,
                IsToday: date == today,
                Items: items.Where(item => item.CalendarDate == date).ToList()));
        }

        return cells;
    }

    /// <summary>Gün kutusuna tıklanınca açılan liste: o güne düşen her şey.</summary>
    public async Task<IReadOnlyList<ICalendarItem>> GetDayAsync(DateOnly date) =>
        await GetItemsBetweenAsync(date, date);

    /// <summary>
    /// Üç sorgunun sonuçlarını tek listede birleştirir: etkinlikler, görevler, notlar.
    /// Liste ICalendarItem türünde olduğu için üç farklı sınıf yan yana durabilir.
    /// first ve last günleri de dahildir. Depo sözleşmeleri aralığın uçlarının dahil olup
    /// olmadığını söylemediği için depolardan iki yana birer gün fazlası istenir, fazlası
    /// burada atılır (NoteService'teki yöntemin aynısı).
    /// </summary>
    private async Task<IReadOnlyList<ICalendarItem>> GetItemsBetweenAsync(DateOnly first, DateOnly last)
    {
        DateOnly from = first.AddDays(-1);
        DateOnly to = last.AddDays(1);

        var items = new List<ICalendarItem>();
        items.AddRange(await _events.GetByDateRangeAsync(from, to));
        items.AddRange(await _todos.GetByDueDateRangeAsync(from, to));
        items.AddRange(await _notes.GetByDateRangeAsync(from, to));

        return items
            .Where(item => item.CalendarDate >= first && item.CalendarDate <= last)
            .ToList();
    }

    /// <summary>Pazartesi = 0, Salı = 1, ..., Pazar = 6. (.NET'te pazar 0 ile başlar.)</summary>
    private static int DaysSinceMonday(DateOnly date) =>
        ((int)date.DayOfWeek + 6) % 7;
}
