using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModularAgenda.Core.Domain;
using ModularAgenda.Data;

namespace ModularAgenda.Tests.Data;

public class AppDbContextTests : IDisposable
{
    // In-memory SQLite veritabanı bağlantı kapanınca silinir; bu yüzden test boyunca açık tutulur.
    private readonly SqliteConnection _connection;

    public AppDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void EnsureCreated_WithAllEntities_DoesNotThrow()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        Exception? exception = Record.Exception(() => context.Database.EnsureCreated());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void SaveChanges_WithNote_NoteCanBeReadBackFromNewContext()
    {
        // Arrange
        var note = new Note(new DateOnly(2026, 11, 25), "Proje toplantısı");
        using (var context = CreateContext())
        {
            context.Database.EnsureCreated();
            context.Notes.Add(note);

            // Act
            context.SaveChanges();
        }

        // Assert: yeni context önbelleği kullanamaz, not veritabanından okunur
        using var readContext = CreateContext();
        Note saved = Assert.Single(readContext.Notes.ToList());
        Assert.Equal(note.Id, saved.Id);
        Assert.Equal(note.Date, saved.Date);
        Assert.Equal(note.Text, saved.Text);
    }
}
