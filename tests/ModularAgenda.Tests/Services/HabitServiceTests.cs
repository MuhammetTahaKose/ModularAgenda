using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Services;
using ModularAgenda.Tests.Fakes;

namespace ModularAgenda.Tests.Services;

public class HabitServiceTests
{
    private readonly FakeRepository<Habit> _habits = new();
    private readonly FakeReminderRepository _reminders = new();
    private readonly HabitService _service;

    public HabitServiceTests()
    {
        _service = new HabitService(_habits, new LinkedItemsCleaner(new FakeNoteRepository(), _reminders));
    }

    [Fact]
    public async Task DeleteAsync_DeletesOnlyThisHabitsReminders()
    {
        // Arrange
        var reading = new TestHabit("Kitap oku");
        var sport = new TestHabit("Spor");
        await _service.AddAsync(reading);
        await _service.AddAsync(sport);
        var readingReminder = TestReminder.Daily(reading, "21:00", new TimeSpan(18, 0, 0));
        var sportReminder = TestReminder.Daily(sport, "07:00", new TimeSpan(4, 0, 0));
        await _reminders.AddAsync(readingReminder);
        await _reminders.AddAsync(sportReminder);

        // Act
        await _service.DeleteAsync(reading);

        // Assert
        Assert.True(reading.IsDeleted);
        Assert.True(readingReminder.IsDeleted);
        Assert.False(sportReminder.IsDeleted);
        Assert.Contains(readingReminder.Id, _reminders.UpdatedIds);
    }
}
