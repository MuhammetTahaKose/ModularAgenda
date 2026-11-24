using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Services;
using ModularAgenda.Core.Services.Strategies;
using ModularAgenda.Tests.Fakes;

namespace ModularAgenda.Tests.Services;

public class TodoServiceTests
{
    private readonly FakeTodoRepository _todos = new();
    private readonly FakeNoteRepository _notes = new();
    private readonly FakeReminderRepository _reminders = new();
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        _service = new TodoService(_todos, new LinkedItemsCleaner(_notes, _reminders));
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsSameTodo()
    {
        // Arrange
        var todo = new TestTodo("Rapor yaz");

        // Act
        await _service.AddAsync(todo);

        // Assert
        Assert.Same(todo, await _service.GetByIdAsync(todo.Id));
    }

    [Fact]
    public async Task DeleteAsync_MarksDeleted_SavesIt_AndHidesTheTodo()
    {
        // Arrange
        var todo = new TestTodo("Silinecek");
        await _service.AddAsync(todo);

        // Act
        await _service.DeleteAsync(todo);

        // Assert
        Assert.True(todo.IsDeleted);
        Assert.Contains(todo.Id, _todos.UpdatedIds); // İşaret koymak yetmez, kaydedilmeli.
        Assert.Null(await _service.GetByIdAsync(todo.Id));
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_AlsoDeletesLinkedNotesAndReminders()
    {
        // Arrange
        var todo = new TestTodo("Ara rapor");
        var otherTodo = new TestTodo("Başka görev");
        await _service.AddAsync(todo);
        await _service.AddAsync(otherTodo);
        var linkedNote = new Note(new DateOnly(2026, 11, 27), "Kaynakları ekle");
        var otherNote = new Note(new DateOnly(2026, 11, 27), "Bu kalmalı");
        await _notes.AddLinkedAsync(linkedNote, todo);
        await _notes.AddLinkedAsync(otherNote, otherTodo);
        var reminder = TestReminder.Once(todo, "27 Kasım 09:00", new DateTime(2026, 11, 27, 6, 0, 0, DateTimeKind.Utc));
        await _reminders.AddAsync(reminder);

        // Act
        await _service.DeleteAsync(todo);

        // Assert
        Assert.True(linkedNote.IsDeleted);
        Assert.True(reminder.IsDeleted);
        Assert.False(otherNote.IsDeleted);
        Assert.Contains(linkedNote.Id, _notes.UpdatedIds);
        Assert.Contains(reminder.Id, _reminders.UpdatedIds);
    }

    [Fact]
    public async Task GetTodosAsync_FiltersFirst_ThenSorts()
    {
        // Arrange
        await _service.AddAsync(new TestTodo("Rapor yaz"));
        await _service.AddAsync(new TestTodo("[x] Bitti"));
        await _service.AddAsync(new TestTodo("Ders çalış"));

        // Act
        IReadOnlyList<TodoItem> result =
            await _service.GetTodosAsync(new HideCheckedFilter(), new BySummarySort());

        // Assert
        Assert.Equal(new[] { "Ders çalış", "Rapor yaz" }, result.Select(t => t.Summary));
    }

    // Testin kendi küçük stratejileri. Servis hangi stratejinin geldiğini bilmeden uygular;
    // Strategy deseninin bütün fikri bu.
    private sealed class HideCheckedFilter : ITodoFilterStrategy
    {
        public string DisplayName => "İşaretlileri gizle";
        public bool Matches(TodoItem todo) => !todo.Summary.StartsWith("[x]", StringComparison.Ordinal);
    }

    private sealed class BySummarySort : ITodoSortStrategy
    {
        public string DisplayName => "Ada göre";
        public IEnumerable<TodoItem> Sort(IEnumerable<TodoItem> todos) =>
            todos.OrderBy(t => t.Summary, StringComparer.Ordinal);
    }
}
