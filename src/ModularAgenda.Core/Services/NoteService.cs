using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;

namespace ModularAgenda.Core.Services;

/// <summary>
/// Notların servisi. Aylık takvim ekranı bununla çalışır: ayın notlarını getirir (not olan
/// günlere nokta koymak için), tıklanan günün notunu getirir ve "Kaydet" düğmesiyle günün
/// notunu yazar. Şimdilik her güne tek not düşer.
/// </summary>
public class NoteService : CrudService<Note>
{
    private readonly INoteRepository _notes;

    public NoteService(INoteRepository notes) : base(notes)
    {
        _notes = notes;
    }

    /// <summary>
    /// Ayın notları, tarihe göre sıralı. Takvimde hangi günlerde not olduğunu göstermek için.
    /// </summary>
    public async Task<IReadOnlyList<Note>> GetNotesForMonthAsync(int year, int month)
    {
        var firstDay = new DateOnly(year, month, 1);
        var lastDay = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        return await GetNotesBetweenAsync(firstDay, lastDay);
    }

    /// <summary>
    /// O günün notu; yoksa null. Günde birden fazla not varsa (örneğin AddAsync ile ayrıca
    /// eklenmişse) en eski not döner; SaveNoteForDayAsync de aynı notu günceller.
    /// </summary>
    public async Task<Note?> GetNoteForDayAsync(DateOnly date)
    {
        IReadOnlyList<Note> notes = await GetNotesBetweenAsync(date, date);
        return notes.FirstOrDefault();
    }

    /// <summary>
    /// "Kaydet" düğmesi: o günün notu varsa metnini günceller, yoksa yeni not oluşturur.
    /// Metin boşsa Note bir DomainException fırlatır ve hiçbir şey kaydedilmez.
    /// </summary>
    /// <returns>Kaydedilen not.</returns>
    public async Task<Note> SaveNoteForDayAsync(DateOnly date, string text)
    {
        Note? note = await GetNoteForDayAsync(date);

        if (note is null)
        {
            // Metin boşsa exception burada fırlar ve AddAsync satırına hiç gelinmez.
            note = new Note(date, text);
            await AddAsync(note);
        }
        else
        {
            // Aynı şekilde: metin boşsa UpdateAsync satırına gelinmez, eski not olduğu gibi kalır.
            note.UpdateText(text);
            await UpdateAsync(note);
        }

        return note;
    }

    /// <summary>Bir göreve ya da etkinliğe bağlı notlar.</summary>
    public async Task<IReadOnlyList<Note>> GetForTargetAsync(INoteable target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return await _notes.GetByTargetAsync(target.Id);
    }

    /// <summary>
    /// first ile last arasındaki notlar (iki gün de dahil), tarihe göre sıralı.
    /// INoteRepository, GetByDateRangeAsync'te aralığın uçlarının dahil olup olmadığını
    /// söylemiyor. Bu yüzden depodan iki yana birer gün fazlasını istiyor, sonucu kendimiz
    /// süzüyoruz: depo uçları dahil etse de etmese de sonuç doğru olur.
    /// </summary>
    private async Task<IReadOnlyList<Note>> GetNotesBetweenAsync(DateOnly first, DateOnly last)
    {
        IReadOnlyList<Note> notes = await _notes.GetByDateRangeAsync(first.AddDays(-1), last.AddDays(1));
        return notes
            .Where(note => note.CalendarDate >= first && note.CalendarDate <= last)
            .OrderBy(note => note.CalendarDate)
            .ThenBy(note => note.CreatedAtUtc)
            .ToList();
    }
}
