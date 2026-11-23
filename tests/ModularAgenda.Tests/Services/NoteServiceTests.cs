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

    [Fact]
    public async Task GetForTargetAsync_ReturnsOnlyThatTargetsNotes()
    {
        // Arrange
        var todo = new TestTodo("Ara rapor");
        var otherTodo = new TestTodo("Başka görev");
        var note = new TestNote("Kaynakları ekle");
        await _notes.AddLinkedAsync(note, todo);
        await _notes.AddLinkedAsync(new TestNote("Başka not"), otherTodo);

        // Act
        var result = await _service.GetForTargetAsync(todo);

        // Assert
        Assert.Same(note, Assert.Single(result));
    }

    [Fact]
    public async Task GetForDayAsync_ReturnsOnlyThatDaysNotes()
    {
        // Arrange
        var day = new DateOnly(2026, 12, 3);
        var note = new TestNote("Annemi ara", day);
        await _service.AddAsync(note);
        await _service.AddAsync(new TestNote("Ertesi gün", day.AddDays(1)));
        await _notes.AddLinkedAsync(new TestNote("Göreve bağlı, takvimde görünmez"), new TestTodo("Görev"));

        // Act
        var result = await _service.GetForDayAsync(day);

        // Assert
        Assert.Same(note, Assert.Single(result));
    }

    [Fact]
    public async Task DeleteAsync_MarksDeletedAndSavesIt()
    {
        // Arrange
        var note = new TestNote("Silinecek", new DateOnly(2026, 12, 3));
        await _service.AddAsync(note);

        // Act
        await _service.DeleteAsync(note);

        // Assert
        Assert.True(note.IsDeleted);
        Assert.Contains(note.Id, _notes.UpdatedIds);
        Assert.Empty(await _service.GetForDayAsync(new DateOnly(2026, 12, 3)));
    }
}
