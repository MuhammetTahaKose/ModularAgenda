using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Tests.Fakes;

/// <summary>Tarih aralığı sorguları için ortak kural: aralığın iki ucu da dahil.</summary>
internal static class CalendarRange
{
    public static bool Contains(ICalendarItem item, DateOnly start, DateOnly end) =>
        item.CalendarDate is DateOnly day && day >= start && day <= end;
}

public class FakeTodoRepository : FakeRepository<TodoItem>, ITodoRepository
{
    public Task<IReadOnlyList<TodoItem>> GetByDueDateRangeAsync(DateOnly start, DateOnly end) =>
        Task.FromResult<IReadOnlyList<TodoItem>>(
            Active().Where(todo => CalendarRange.Contains(todo, start, end)).ToList());
}

public class FakeCalendarEventRepository : FakeRepository<CalendarEvent>, ICalendarEventRepository
{
    public Task<IReadOnlyList<CalendarEvent>> GetByDateRangeAsync(DateOnly start, DateOnly end) =>
        Task.FromResult<IReadOnlyList<CalendarEvent>>(
            Active().Where(e => CalendarRange.Contains(e, start, end)).ToList());
}

public class FakeNoteRepository : FakeRepository<Note>, INoteRepository
{
    // Note sınıfında henüz "bağlı olduğu kayıt" alanı yok (Kadir ekleyecek).
    // O gelene kadar hangi notun hangi kayda bağlı olduğunu burada tutuyoruz.
    private readonly Dictionary<Guid, Guid> _targetOfNote = new();

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
            Active().Where(n => CalendarRange.Contains(n, start, end)).ToList());
}

public class FakeReminderRepository : FakeRepository<Reminder>, IReminderRepository
{
    public Task<IReadOnlyList<Reminder>> GetByTargetAsync(Guid targetId) =>
        Task.FromResult<IReadOnlyList<Reminder>>(
            Active().Where(r => r.TargetId == targetId).ToList());
}
