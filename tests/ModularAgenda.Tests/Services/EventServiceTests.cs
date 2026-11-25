using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Services;
using ModularAgenda.Tests.Fakes;

namespace ModularAgenda.Tests.Services;

public class EventServiceTests
{
    private readonly FakeCalendarEventRepository _events = new();
    private readonly FakeNoteRepository _notes = new();
    private readonly FakeReminderRepository _reminders = new();
    private readonly EventService _service;

    public EventServiceTests()
    {
        _service = new EventService(_events, new LinkedItemsCleaner(_notes, _reminders));
    }

    [Fact]
    public async Task DeleteAsync_MarksDeletedAndSavesIt()
    {
        // Arrange
        var meeting = new TestEvent("14:00 Toplantı", new DateOnly(2026, 11, 24));
        await _service.AddAsync(meeting);

        // Act
        await _service.DeleteAsync(meeting);

        // Assert
        Assert.True(meeting.IsDeleted);
        Assert.Contains(meeting.Id, _events.UpdatedIds);
        Assert.Null(await _service.GetByIdAsync(meeting.Id));
    }

    [Fact]
    public async Task DeleteAsync_AlsoDeletesLinkedNotesAndReminders()
    {
        // Arrange
        var meeting = new TestEvent("14:00 Toplantı", new DateOnly(2026, 11, 24));
        await _service.AddAsync(meeting);
        var note = new Note(new DateOnly(2026, 11, 24), "Sunumu getir");
        await _notes.AddLinkedAsync(note, meeting);
        var reminder = TestReminder.Once(meeting, "24 Kasım 13:30", new DateTime(2026, 11, 24, 10, 30, 0, DateTimeKind.Utc));
        await _reminders.AddAsync(reminder);

        // Act
        await _service.DeleteAsync(meeting);

        // Assert
        Assert.True(note.IsDeleted);
        Assert.True(reminder.IsDeleted);
    }
}
