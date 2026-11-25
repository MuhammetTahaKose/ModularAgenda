using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Data;
using ModularAgenda.Data.Repositories;

namespace ModularAgenda.Tests.Data;

public class EfNoteRepositoryTests : IDisposable
{
    // In-memory SQLite veritabanı bağlantı kapanınca silinir; bu yüzden test boyunca açık tutulur.
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly EfNoteRepository _repository;

    public EfNoteRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Uygulamadaki gibi tek bir context kullanılır (App'te Singleton).
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new EfNoteRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static DateOnly Nov(int day) => new(2026, 11, day);

    [Fact]
    public async Task GetByIdAsync_AfterAddAsync_ReturnsNote()
    {
        // Arrange
        var note = new Note(Nov(10), "Alışveriş listesi");
        await _repository.AddAsync(note);

        // Act
        Note? found = await _repository.GetByIdAsync(note.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(note.Id, found.Id);
        Assert.Equal(note.Text, found.Text);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNoteDeleted_ReturnsNull()
    {
        // Arrange
        var note = new Note(Nov(10), "Silinecek not");
        await _repository.AddAsync(note);
        note.Delete();
        await _repository.UpdateAsync(note);

        // Act
        Note? found = await _repository.GetByIdAsync(note.Id);

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoteDeleted_DoesNotContainIt()
    {
        // Arrange
        var kept = new Note(Nov(10), "Kalan not");
        var deleted = new Note(Nov(11), "Silinecek not");
        await _repository.AddAsync(kept);
        await _repository.AddAsync(deleted);
        deleted.Delete();
        await _repository.UpdateAsync(deleted);

        // Act
        IReadOnlyList<Note> all = await _repository.GetAllAsync();

        // Assert
        Note only = Assert.Single(all);
        Assert.Equal(kept.Id, only.Id);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithNotesInsideAndOutside_ReturnsOnlyInsideIncludingBoundaries()
    {
        // Arrange: aralık 10–20 Kasım; 9 ve 21 dışarıda, 10 ve 20 sınırda
        var before = new Note(Nov(9), "Önce");
        var start = new Note(Nov(10), "İlk gün");
        var middle = new Note(Nov(15), "Orta");
        var end = new Note(Nov(20), "Son gün");
        var after = new Note(Nov(21), "Sonra");
        foreach (Note note in new[] { before, start, middle, end, after })
            await _repository.AddAsync(note);

        // Act
        IReadOnlyList<Note> result = await _repository.GetByDateRangeAsync(Nov(10), Nov(20));

        // Assert
        Assert.Equal(
            new[] { start.Id, middle.Id, end.Id }.OrderBy(id => id),
            result.Select(n => n.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetByDateRangeAsync_WhenNoteInRangeDeleted_DoesNotReturnIt()
    {
        // Arrange
        var kept = new Note(Nov(12), "Kalan not");
        var deleted = new Note(Nov(13), "Silinecek not");
        await _repository.AddAsync(kept);
        await _repository.AddAsync(deleted);
        deleted.Delete();
        await _repository.UpdateAsync(deleted);

        // Act
        IReadOnlyList<Note> result = await _repository.GetByDateRangeAsync(Nov(10), Nov(20));

        // Assert
        Note only = Assert.Single(result);
        Assert.Equal(kept.Id, only.Id);
    }
}
