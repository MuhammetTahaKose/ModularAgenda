using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Exceptions;
using ModularAgenda.Core.Services;
using ModularAgenda.Tests.Fakes;

namespace ModularAgenda.Tests.Services;

public class NoteServiceTests
{
    private readonly FakeNoteRepository _notes = new();
    private readonly NoteService _service;

    public NoteServiceTests()
    {
        _service = new NoteService(_notes);
    }

    private static DateOnly Nov(int day) => new(2026, 11, day);

    // ---- GetNotesForMonthAsync: takvimde nokta konacak günler ----

    [Fact]
    public async Task GetNotesForMonthAsync_ReturnsOnlyThatMonthsNotes_InDateOrder()
    {
        // Arrange: ayın ilk ve son günü dahil, komşu aylar hariç; ekleme sırası bilerek karışık
        await _service.AddAsync(new Note(Nov(30), "Ayın son günü"));
        await _service.AddAsync(new Note(new DateOnly(2026, 10, 31), "Ekim"));
        await _service.AddAsync(new Note(Nov(15), "Ayın ortası"));
        await _service.AddAsync(new Note(new DateOnly(2026, 12, 1), "Aralık"));
        await _service.AddAsync(new Note(Nov(1), "Ayın ilk günü"));

        // Act
        IReadOnlyList<Note> result = await _service.GetNotesForMonthAsync(2026, 11);

        // Assert
        Assert.Equal(new[] { "Ayın ilk günü", "Ayın ortası", "Ayın son günü" }, result.Select(n => n.Text));
    }

    [Fact]
    public async Task GetNotesForMonthAsync_December_EndsAtTheEndOfTheYear()
    {
        // Arrange: aralıktan sonraki ay, gelecek yılın ocak ayı
        await _service.AddAsync(new Note(new DateOnly(2026, 12, 31), "Yılbaşı gecesi"));
        await _service.AddAsync(new Note(new DateOnly(2027, 1, 1), "Yeni yıl"));

        // Act
        IReadOnlyList<Note> result = await _service.GetNotesForMonthAsync(2026, 12);

        // Assert
        Assert.Equal("Yılbaşı gecesi", Assert.Single(result).Text);
    }

    // ---- GetNoteForDayAsync: güne tıklanınca TextBox'a gelecek not ----

    [Fact]
    public async Task GetNoteForDayAsync_NoNote_ReturnsNull()
    {
        Assert.Null(await _service.GetNoteForDayAsync(Nov(24)));
    }

    [Fact]
    public async Task GetNoteForDayAsync_ReturnsThatDaysNote_NotTheNeighbours()
    {
        // Arrange
        var note = new Note(Nov(24), "Annemi ara");
        await _service.AddAsync(new Note(Nov(23), "Dün"));
        await _service.AddAsync(note);
        await _service.AddAsync(new Note(Nov(25), "Yarın"));

        // Act
        Note? result = await _service.GetNoteForDayAsync(Nov(24));

        // Assert
        Assert.Same(note, result);
    }

    [Fact]
    public async Task GetNoteForDayAsync_DayHasTwoNotes_ReturnsTheOlderOne()
    {
        // Arrange: Kaydet günde tek not tutar, ama AddAsync ile ikinci not eklenebilir.
        // O zaman Get ve Save aynı (en eski) notla çalışmalı.
        var older = new Note(Nov(24), "Eski");
        await _service.AddAsync(older);
        await _service.AddAsync(new Note(Nov(24), "Yeni"));

        // Act
        Note? result = await _service.GetNoteForDayAsync(Nov(24));

        // Assert
        Assert.Same(older, result);
    }

    // ---- SaveNoteForDayAsync: "Kaydet" düğmesi ----

    [Fact]
    public async Task SaveNoteForDayAsync_NoNoteYet_CreatesNoteForThatDay()
    {
        // Act
        Note saved = await _service.SaveNoteForDayAsync(Nov(24), "Annemi ara");

        // Assert
        Assert.Equal("Annemi ara", saved.Text);
        Assert.Equal(Nov(24), saved.CalendarDate);
        Assert.Same(saved, await _service.GetNoteForDayAsync(Nov(24)));
    }

    [Fact]
    public async Task SaveNoteForDayAsync_NoteExists_UpdatesItInsteadOfAddingASecondOne()
    {
        // Arrange
        Note first = await _service.SaveNoteForDayAsync(Nov(24), "Annemi ara");

        // Act
        Note second = await _service.SaveNoteForDayAsync(Nov(24), "Annemi ara, ekmek al");

        // Assert
        Assert.Same(first, second);
        Assert.Equal("Annemi ara, ekmek al", first.Text);
        Assert.Contains(first.Id, _notes.UpdatedIds); // Metni değiştirmek yetmez, kaydedilmeli.
        Assert.Single(await _service.GetAllAsync());
    }

    [Fact]
    public async Task SaveNoteForDayAsync_EmptyText_ThrowsDomainException_AndAddsNothing()
    {
        // Act + Assert: boş metni Note reddeder (Kadir'in kuralı), servis de hiçbir şey eklemez
        await Assert.ThrowsAnyAsync<DomainException>(() => _service.SaveNoteForDayAsync(Nov(24), ""));
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task SaveNoteForDayAsync_EmptyTextForExistingNote_ThrowsAndSavesNothing()
    {
        // Arrange
        Note existing = await _service.SaveNoteForDayAsync(Nov(24), "Annemi ara");

        // Act + Assert
        await Assert.ThrowsAnyAsync<DomainException>(() => _service.SaveNoteForDayAsync(Nov(24), ""));
        Assert.Empty(_notes.UpdatedIds);
        Assert.Same(existing, Assert.Single(await _service.GetAllAsync()));
    }

    // ---- Depo aralığın uçlarını dahil etse de etmese de sonuç aynı ----
    // Burak'ın InMemoryNoteRepository'si henüz yok ve sözleşme bunu söylemiyor.

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetNotesForMonthAsync_WorksWhetherRepositoryIncludesRangeEndsOrNot(bool excludesEnds)
    {
        // Arrange
        var notes = new FakeNoteRepository { ExcludesRangeEnds = excludesEnds };
        var service = new NoteService(notes);
        await service.AddAsync(new Note(Nov(1), "İlk gün"));
        await service.AddAsync(new Note(Nov(30), "Son gün"));
        await service.AddAsync(new Note(new DateOnly(2026, 12, 1), "Aralık"));

        // Act
        IReadOnlyList<Note> result = await service.GetNotesForMonthAsync(2026, 11);

        // Assert
        Assert.Equal(new[] { "İlk gün", "Son gün" }, result.Select(n => n.Text));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveNoteForDayAsync_WorksWhetherRepositoryIncludesRangeEndsOrNot(bool excludesEnds)
    {
        // Arrange: uçları dahil etmeyen depoda tek günlük sorgu boş dönerse her Kaydet yeni not açardı
        var notes = new FakeNoteRepository { ExcludesRangeEnds = excludesEnds };
        var service = new NoteService(notes);
        await service.SaveNoteForDayAsync(Nov(24), "Annemi ara");

        // Act
        await service.SaveNoteForDayAsync(Nov(24), "Annemi ara, ekmek al");

        // Assert
        Note note = Assert.Single(await service.GetAllAsync());
        Assert.Equal("Annemi ara, ekmek al", note.Text);
    }

    // ---- Bağlı notlar ve silme ----

    [Fact]
    public async Task GetForTargetAsync_ReturnsOnlyThatTargetsNotes()
    {
        // Arrange
        var todo = new TestTodo("Ara rapor");
        var note = new Note(Nov(27), "Kaynakları ekle");
        await _notes.AddLinkedAsync(note, todo);
        await _notes.AddLinkedAsync(new Note(Nov(27), "Başka not"), new TestTodo("Başka görev"));

        // Act
        var result = await _service.GetForTargetAsync(todo);

        // Assert
        Assert.Same(note, Assert.Single(result));
    }

    [Fact]
    public async Task DeleteAsync_MarksDeletedAndSavesIt()
    {
        // Arrange
        Note note = await _service.SaveNoteForDayAsync(Nov(24), "Silinecek");

        // Act
        await _service.DeleteAsync(note);

        // Assert
        Assert.True(note.IsDeleted);
        Assert.Contains(note.Id, _notes.UpdatedIds);
        Assert.Null(await _service.GetNoteForDayAsync(Nov(24)));
    }
}
