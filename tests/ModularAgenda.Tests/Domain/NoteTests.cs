using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Exceptions;

namespace ModularAgenda.Tests.Domain;

public class NoteTests
{
    private static readonly DateOnly SampleDate = new(2026, 11, 24);

    // DateTime.UtcNow'un iki çağrı arasında kesin ilerlemesi için kısa bekleme.
    private static void WaitForClockToAdvance() => Thread.Sleep(15);

    [Fact]
    public void Constructor_WithValidText_SetsDateAndText()
    {
        // Arrange & Act
        var note = new Note(SampleDate, "Doktor randevusu");

        // Assert
        Assert.Equal(SampleDate, note.CalendarDate);
        Assert.Equal("Doktor randevusu", note.Text);
        Assert.False(note.IsDeleted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyText_ThrowsEmptyNoteTextException(string? text)
    {
        Assert.Throws<EmptyNoteTextException>(() => new Note(SampleDate, text!));
    }

    [Fact]
    public void UpdateText_WithValidText_ChangesTextAndAdvancesUpdatedAt()
    {
        // Arrange
        var note = new Note(SampleDate, "İlk metin");
        var updatedBefore = note.UpdatedAtUtc;
        WaitForClockToAdvance();

        // Act
        note.UpdateText("Yeni metin");

        // Assert
        Assert.Equal("Yeni metin", note.Text);
        Assert.True(note.UpdatedAtUtc > updatedBefore);
    }

    [Fact]
    public void UpdateText_WithEmptyText_ThrowsAndKeepsOldText()
    {
        // Arrange
        var note = new Note(SampleDate, "İlk metin");
        var updatedBefore = note.UpdatedAtUtc;

        // Act & Assert
        Assert.Throws<EmptyNoteTextException>(() => note.UpdateText("  "));
        Assert.Equal("İlk metin", note.Text);
        Assert.Equal(updatedBefore, note.UpdatedAtUtc);
    }

    [Fact]
    public void Summary_WithShortText_ReturnsFullText()
    {
        var note = new Note(SampleDate, "Kısa not");

        Assert.Equal("Kısa not", note.Summary);
    }

    [Fact]
    public void Summary_WithLongText_ReturnsFirst30CharactersWithEllipsis()
    {
        var text = new string('a', 50);
        var note = new Note(SampleDate, text);

        Assert.Equal(new string('a', 30) + "…", note.Summary);
    }
}
