using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Tests.Fakes;

/// <summary>
/// Tarih aralığı sorguları için ortak kural. Normalde aralığın iki ucu da dahildir.
/// excludeEnds true ise uçları dahil etmeyen bir depo gibi davranır; servislerin iki
/// durumda da doğru çalıştığını göstermek için. (Depo sözleşmesi bunu henüz söylemiyor.)
/// </summary>
internal static class CalendarRange
{
    public static bool Contains(ICalendarItem item, DateOnly start, DateOnly end, bool excludeEnds) =>
        item.CalendarDate is DateOnly day &&
        (excludeEnds ? day > start && day < end : day >= start && day <= end);
}

public class FakeTodoRepository : FakeRepository<TodoItem>, ITodoRepository
{
    /// <summary>true ise tarih aralığının uçlarını dahil etmez.</summary>
    public bool ExcludesRangeEnds { get; init; }

    public Task<IReadOnlyList<TodoItem>> GetByDueDateRangeAsync(DateOnly start, DateOnly end) =>
        Task.FromResult<IReadOnlyList<TodoItem>>(
            Active().Where(todo => CalendarRange.Contains(todo, start, end, ExcludesRangeEnds)).ToList());
}

public class FakeCalendarEventRepository : FakeRepository<CalendarEvent>, ICalendarEventRepository
{
    /// <summary>true ise tarih aralığının uçlarını dahil etmez.</summary>
    public bool ExcludesRangeEnds { get; init; }

    public Task<IReadOnlyList<CalendarEvent>> GetByDateRangeAsync(DateOnly start, DateOnly end) =>
        Task.FromResult<IReadOnlyList<CalendarEvent>>(
            Active().Where(e => CalendarRange.Contains(e, start, end, ExcludesRangeEnds)).ToList());
}

public class FakeNoteRepository : FakeRepository<Note>, INoteRepository
{
    // Note'ta şimdilik "bağlı olduğu kayıt" (TargetId) yok; prototip belgesine göre sonra gelecek.
    // O gelene kadar hangi notun hangi kayda bağlı olduğunu burada tutuyoruz.
    private readonly Dictionary<Guid, Guid> _targetOfNote = new();

    /// <summary>true ise tarih aralığının uçlarını dahil etmez.</summary>
    public bool ExcludesRangeEnds { get; init; }

    public async Task AddLinkedAsync(Note note, INoteable target)
    {
        await AddAsync(note);
        _targetOfNote[note.Id] = target.Id;
    }

    public Task<IReadOnlyList<Note>> GetByTargetAsync(Guid targetId) =>
        Task.FromResult<IReadOnlyList<Note>>(
            Active().Where(n => _targetOfNote.TryGetValue(n.Id, out Guid t) && t == targetId).ToList());

    public Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateOnly start, DateOnly end) =>
        Task.FromResult<IReadOnlyList<Note>>(
            Active().Where(n => CalendarRange.Contains(n, start, end, ExcludesRangeEnds)).ToList());
}

public class FakeReminderRepository : FakeRepository<Reminder>, IReminderRepository
{
    public Task<IReadOnlyList<Reminder>> GetByTargetAsync(Guid targetId) =>
        Task.FromResult<IReadOnlyList<Reminder>>(
            Active().Where(r => r.TargetId == targetId).ToList());
}
