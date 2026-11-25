using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services;

/// <summary>
/// Bir kayıt silinince ona bağlı notları ve hatırlatıcıları da siler (Karar K2.6).
/// Parametre türleri interface olduğu için yanlış kullanım derlenmez: alışkanlık
/// INoteable olmadığından DeleteNotesOfAsync(habit) yazılamaz.
/// </summary>
public class LinkedItemsCleaner
{
    private readonly INoteRepository _notes;
    private readonly IReminderRepository _reminders;

    public LinkedItemsCleaner(INoteRepository notes, IReminderRepository reminders)
    {
        ArgumentNullException.ThrowIfNull(notes);
        ArgumentNullException.ThrowIfNull(reminders);
        _notes = notes;
        _reminders = reminders;
    }

    public async Task DeleteNotesOfAsync(INoteable target)
    {
        ArgumentNullException.ThrowIfNull(target);
        foreach (Note note in await _notes.GetByTargetAsync(target.Id))
        {
            note.Delete();
            await _notes.UpdateAsync(note);
        }
    }

    public async Task DeleteRemindersOfAsync(IRemindable target)
    {
        ArgumentNullException.ThrowIfNull(target);
        foreach (Reminder reminder in await _reminders.GetByTargetAsync(target.Id))
        {
            reminder.Delete();
            await _reminders.UpdateAsync(reminder);
        }
    }
}
