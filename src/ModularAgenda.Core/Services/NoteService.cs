using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services;

/// <summary>
/// Notların servisi. Bir notun neye bağlanacağı not oluşturulurken constructor ile
/// belirlenir (bölüm 4.3); servis notları bağlı oldukları kayda ya da güne göre getirir.
/// </summary>
public class NoteService : CrudService<Note>
{
    private readonly INoteRepository _notes;

    public NoteService(INoteRepository notes) : base(notes)
    {
        _notes = notes;
    }

    /// <summary>Bir göreve ya da etkinliğe bağlı notlar.</summary>
    public async Task<IReadOnlyList<Note>> GetForTargetAsync(INoteable target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return await _notes.GetByTargetAsync(target.Id);
    }

    /// <summary>Bir güne bağlı notlar.</summary>
    public async Task<IReadOnlyList<Note>> GetForDayAsync(DateOnly day) =>
        await _notes.GetByDateRangeAsync(day, day);
}
