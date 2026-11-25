using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data.Repositories;

public class InMemoryNoteRepository : INoteRepository
{
    private readonly List<Note> _notes = new();

    public Task AddAsync(Note item)
    {
        _notes.Add(item);
        return Task.CompletedTask;
    }

    public Task<Note?> GetByIdAsync(Guid id)
    {
        var note = _notes.FirstOrDefault(n => n.Id == id && !n.IsDeleted);
        return Task.FromResult(note);
    }

    public Task<IReadOnlyList<Note>> GetAllAsync()
    {
        var list = _notes.Where(n => !n.IsDeleted).ToList().AsReadOnly();
        return Task.FromResult<IReadOnlyList<Note>>(list);
    }

    public Task UpdateAsync(Note item)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == item.Id);
        if (existing != null)
        {
            var index = _notes.IndexOf(existing);
            _notes[index] = item;
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Note>> GetByTargetAsync(Guid targetId)
    {
        // Şimdilik boş liste döndürebilir (istenildiği gibi)
        var list = new List<Note>().AsReadOnly();
        return Task.FromResult<IReadOnlyList<Note>>(list);
    }

    public Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        // Silinmişleri atlar, aralıktaki notları döndürür
        var list = _notes
            .Where(n => !n.IsDeleted && n.CalendarDate.HasValue && n.CalendarDate.Value >= start && n.CalendarDate.Value <= end)
            .ToList()
            .AsReadOnly();
        return Task.FromResult<IReadOnlyList<Note>>(list);
    }
}
