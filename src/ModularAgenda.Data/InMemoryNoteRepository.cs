using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Data;

public sealed class InMemoryNoteRepository : INoteRepository
{
    private readonly Dictionary<Guid, Note> _notes = new();
    private readonly object _sync = new();

    public Task AddAsync(Note item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_sync) _notes.Add(item.Id, item);
        return Task.CompletedTask;
    }

    public Task<Note?> GetByIdAsync(Guid id)
    {
        lock (_sync)
            return Task.FromResult(_notes.TryGetValue(id, out var note) && !note.IsDeleted ? note : null);
    }

    public Task<IReadOnlyList<Note>> GetAllAsync()
    {
        lock (_sync)
            return Task.FromResult<IReadOnlyList<Note>>(_notes.Values.Where(note => !note.IsDeleted).ToList());
    }

    public Task UpdateAsync(Note item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_sync)
        {
            if (!_notes.ContainsKey(item.Id))
                throw new InvalidOperationException("Güncellenecek not bulunamadı.");
            _notes[item.Id] = item;
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Note>> GetByTargetAsync(Guid targetId) =>
        Task.FromResult<IReadOnlyList<Note>>(Array.Empty<Note>());

    public Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        lock (_sync)
        {
            var notes = _notes.Values
                .Where(note => !note.IsDeleted && note.Date >= start && note.Date <= end)
                .OrderBy(note => note.Date)
                .ThenBy(note => note.CreatedAtUtc)
                .ToList();
            return Task.FromResult<IReadOnlyList<Note>>(notes);
        }
    }
}
