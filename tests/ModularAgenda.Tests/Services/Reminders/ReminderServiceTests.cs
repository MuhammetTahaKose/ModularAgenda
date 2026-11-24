using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Services.Reminders;
using ModularAgenda.Tests.Fakes;

namespace ModularAgenda.Tests.Services.Reminders;

public class ReminderServiceTests
{
    // Servis bu andan itibaren kontrol eder; ondan önce çalması gerekenler gösterilmez.
    private static readonly DateTime Start = new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeReminderRepository _reminders = new();
    private readonly FakeTodoRepository _todos = new();
    private readonly FakeCalendarEventRepository _events = new();
    private readonly FakeRepository<Habit> _habits = new();
    private readonly RecordingObserver _observer = new();
    private readonly ReminderService _service;

    public ReminderServiceTests()
    {
        _service = new ReminderService(_reminders, _todos, _events, _habits, checkFromUtc: Start);
        _service.Subscribe(_observer);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_DueReminder_SendsCombinedMessage()
    {
        // Arrange: belgedeki örnek, "21:00, Kitap oku"
        var habit = new TestHabit("Kitap oku");
        await _habits.AddAsync(habit);
        await _reminders.AddAsync(TestReminder.Daily(habit, "21:00", new TimeSpan(12, 5, 0)));

        // Act
        int shown = await _service.CheckDueRemindersAsync(Start.AddMinutes(10));

        // Assert
        Assert.Equal(1, shown);
        ReminderAlert alert = Assert.Single(_observer.Received);
        Assert.Equal("21:00, Kitap oku", alert.Message);
        Assert.Same(habit, alert.Target);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_FutureReminder_IsNotShown()
    {
        // Arrange
        var todo = new TestTodo("Rapor yaz");
        await _todos.AddAsync(todo);
        await _reminders.AddAsync(TestReminder.Once(todo, "13:00", Start.AddHours(1)));

        // Act
        int shown = await _service.CheckDueRemindersAsync(Start.AddMinutes(30));

        // Assert
        Assert.Equal(0, shown);
        Assert.Empty(_observer.Received);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_OneTimeReminder_IsShownOnlyOnce()
    {
        // Arrange
        var todo = new TestTodo("Rapor yaz");
        await _todos.AddAsync(todo);
        await _reminders.AddAsync(TestReminder.Once(todo, "12:01", Start.AddMinutes(1)));

        // Act: zamanlayıcı üç kez çalışıyor
        await _service.CheckDueRemindersAsync(Start.AddSeconds(30));
        await _service.CheckDueRemindersAsync(Start.AddSeconds(60));
        await _service.CheckDueRemindersAsync(Start.AddSeconds(90));

        // Assert
        Assert.Single(_observer.Received);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_DailyReminder_IsShownAgainNextDay()
    {
        // Arrange
        var habit = new TestHabit("Spor");
        await _habits.AddAsync(habit);
        await _reminders.AddAsync(TestReminder.Daily(habit, "15:00", new TimeSpan(13, 0, 0)));

        // Act
        await _service.CheckDueRemindersAsync(Start.AddHours(2));            // bugün 13:00 geçti
        await _service.CheckDueRemindersAsync(Start.AddHours(3));            // aynı gün, tekrar yok
        await _service.CheckDueRemindersAsync(Start.AddDays(1).AddHours(2)); // ertesi gün

        // Assert
        Assert.Equal(2, _observer.Received.Count);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_SameTriggerTimeReturnedAgain_IsNotShownTwice()
    {
        // Arrange: GetNextTriggerTime "anda ya da sonra" diye yazılırsa az önce çalan zaman
        // tekrar dönebilir; servis bunu ikinci kez göstermemeli.
        var habit = new TestHabit("Kitap oku");
        await _habits.AddAsync(habit);
        await _reminders.AddAsync(TestReminder.OnceInclusive(habit, "12:01", Start.AddMinutes(1)));

        // Act
        await _service.CheckDueRemindersAsync(Start.AddMinutes(1)); // tam çalma anında kontrol
        await _service.CheckDueRemindersAsync(Start.AddMinutes(2));

        // Assert
        Assert.Single(_observer.Received);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_ReminderFromBeforeStart_IsNotShown()
    {
        // Arrange: uygulama kapalıyken zamanı geçmiş hatırlatıcı
        var todo = new TestTodo("Dünkü görev");
        await _todos.AddAsync(todo);
        await _reminders.AddAsync(TestReminder.Once(todo, "Dün 09:00", Start.AddDays(-1)));

        // Act
        int shown = await _service.CheckDueRemindersAsync(Start.AddMinutes(1));

        // Assert
        Assert.Equal(0, shown);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_TargetDeleted_ReminderStaysSilent()
    {
        // Arrange
        var todo = new TestTodo("İptal edilen görev");
        await _todos.AddAsync(todo);
        await _reminders.AddAsync(TestReminder.Once(todo, "12:05", Start.AddMinutes(5)));
        todo.Delete();

        // Act
        int shown = await _service.CheckDueRemindersAsync(Start.AddMinutes(10));

        // Assert
        Assert.Equal(0, shown);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_FindsTargetsOfEveryType()
    {
        // Arrange: hatırlatıcı göreve, etkinliğe ya da alışkanlığa bağlı olabilir
        var todo = new TestTodo("Rapor yaz");
        var meeting = new TestEvent("14:00 Toplantı", new DateOnly(2026, 11, 24));
        var habit = new TestHabit("Kitap oku");
        await _todos.AddAsync(todo);
        await _events.AddAsync(meeting);
        await _habits.AddAsync(habit);
        // Eklenme sırası bilerek karışık: uyarılar eklenme değil çalma sırasıyla gelmeli.
        await _reminders.AddAsync(TestReminder.Once(habit, "C", Start.AddMinutes(3)));
        await _reminders.AddAsync(TestReminder.Once(todo, "A", Start.AddMinutes(1)));
        await _reminders.AddAsync(TestReminder.Once(meeting, "B", Start.AddMinutes(2)));

        // Act
        await _service.CheckDueRemindersAsync(Start.AddMinutes(5));

        // Assert
        Assert.Equal(
            new[] { "A, Rapor yaz", "B, 14:00 Toplantı", "C, Kitap oku" },
            _observer.Received.Select(a => a.Message));
    }

    [Fact]
    public async Task CheckDueRemindersAsync_EverySubscriberIsNotified_UnsubscribedIsNot()
    {
        // Arrange
        var popup = new RecordingObserver();
        var leaver = new RecordingObserver();
        _service.Subscribe(popup);
        _service.Subscribe(popup); // iki kez abone olmak iki uyarı demek değil
        _service.Subscribe(leaver);
        _service.Unsubscribe(leaver);
        var habit = new TestHabit("Kitap oku");
        await _habits.AddAsync(habit);
        await _reminders.AddAsync(TestReminder.Once(habit, "12:01", Start.AddMinutes(1)));

        // Act
        await _service.CheckDueRemindersAsync(Start.AddMinutes(2));

        // Assert
        Assert.Single(_observer.Received);
        Assert.Single(popup.Received);
        Assert.Empty(leaver.Received);
    }

    [Fact]
    public async Task CheckDueRemindersAsync_ClockGoesBack_NothingIsShownTwice()
    {
        // Arrange
        var habit = new TestHabit("Kitap oku");
        await _habits.AddAsync(habit);
        await _reminders.AddAsync(TestReminder.Once(habit, "12:01", Start.AddMinutes(1)));
        await _service.CheckDueRemindersAsync(Start.AddMinutes(2));

        // Act: bilgisayarın saati geri alındı, sonra yeniden ilerledi
        await _service.CheckDueRemindersAsync(Start);
        await _service.CheckDueRemindersAsync(Start.AddMinutes(3));

        // Assert
        Assert.Single(_observer.Received);
    }
}
