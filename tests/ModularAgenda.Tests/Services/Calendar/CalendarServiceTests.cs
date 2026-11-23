using ModularAgenda.Core.Services.Calendar;
using ModularAgenda.Tests.Fakes;

namespace ModularAgenda.Tests.Services.Calendar;

public class CalendarServiceTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);

    private readonly FakeCalendarEventRepository _events = new();
    private readonly FakeTodoRepository _todos = new();
    private readonly FakeNoteRepository _notes = new();
    private readonly CalendarService _calendar;

    public CalendarServiceTests()
    {
        _calendar = new CalendarService(_events, _todos, _notes);
    }

    private static DateOnly Nov(int day) => new(2026, 11, day);

    private static CalendarDay Cell(IReadOnlyList<CalendarDay> grid, DateOnly date) =>
        grid.Single(c => c.Date == date);

    [Fact]
    public async Task GetMonthGridAsync_Has42Cells_StartingOnMonday()
    {
        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        Assert.Equal(CalendarService.GridCellCount, grid.Count);
        Assert.Equal(DayOfWeek.Monday, grid[0].Date.DayOfWeek);
    }

    [Fact]
    public async Task GetMonthGridAsync_November2026_IsPaddedWithOctoberAndDecember()
    {
        // 1 Kasım 2026 pazar: ızgara önceki pazartesiden, 26 Ekim'den başlar.
        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        Assert.Equal(new DateOnly(2026, 10, 26), grid[0].Date);
        Assert.False(grid[0].IsInCurrentMonth);
        Assert.Equal(Nov(1), grid[6].Date);
        Assert.True(grid[6].IsInCurrentMonth);
        Assert.Equal(new DateOnly(2026, 12, 6), grid[^1].Date);
        Assert.Equal(30, grid.Count(c => c.IsInCurrentMonth));
    }

    [Fact]
    public async Task GetMonthGridAsync_MonthStartingOnMonday_HasNoLeadingPadding()
    {
        // 1 Şubat 2027 pazartesi.
        var grid = await _calendar.GetMonthGridAsync(2027, 2, Today);

        Assert.Equal(new DateOnly(2027, 2, 1), grid[0].Date);
        Assert.True(grid[0].IsInCurrentMonth);
    }

    [Fact]
    public async Task GetMonthGridAsync_MarksOnlyToday()
    {
        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        CalendarDay today = Assert.Single(grid, c => c.IsToday);
        Assert.Equal(Today, today.Date);
    }

    [Fact]
    public async Task GetMonthGridAsync_PutsEventsTodosAndNotesTogetherOnTheirDay()
    {
        // Arrange: aynı güne üç farklı tür
        await _events.AddAsync(new TestEvent("14:00 Toplantı", Nov(24)));
        await _todos.AddAsync(new TestTodo("Ara rapor", dueDate: Nov(24)));
        await _notes.AddAsync(new TestNote("Annemi ara", Nov(24)));
        await _todos.AddAsync(new TestTodo("Başka gün", dueDate: Nov(27)));

        // Act
        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        // Assert: kutu türleri bilmeden hepsinin Summary'sini çizebilir
        Assert.Equal(
            new[] { "14:00 Toplantı", "Ara rapor", "Annemi ara" },
            Cell(grid, Nov(24)).Items.Select(i => i.Summary));
        Assert.Single(Cell(grid, Nov(27)).Items);
        Assert.False(Cell(grid, Nov(25)).HasItems);
    }

    [Fact]
    public async Task GetMonthGridAsync_FillsPaddingDaysToo_IncludingTheLastCell()
    {
        // Arrange: ızgaranın ilk ve son kutusu (aralığın iki ucu da dahil olmalı)
        await _events.AddAsync(new TestEvent("Ekim sonu", new DateOnly(2026, 10, 26)));
        await _events.AddAsync(new TestEvent("Aralık başı", new DateOnly(2026, 12, 6)));

        // Act
        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        // Assert
        Assert.Single(grid[0].Items);
        Assert.Single(grid[^1].Items);
    }

    [Fact]
    public async Task GetMonthGridAsync_TodoWithoutDueDate_IsNotOnTheCalendar()
    {
        await _todos.AddAsync(new TestTodo("Tarihsiz görev"));

        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        Assert.DoesNotContain(grid, c => c.HasItems);
    }

    [Fact]
    public async Task GetDayAsync_ReturnsOnlyThatDaysItems()
    {
        // Arrange
        await _events.AddAsync(new TestEvent("14:00 Toplantı", Nov(24)));
        await _notes.AddAsync(new TestNote("Annemi ara", Nov(24)));
        await _events.AddAsync(new TestEvent("Ertesi gün", Nov(25)));

        // Act
        var items = await _calendar.GetDayAsync(Nov(24));

        // Assert
        Assert.Equal(new[] { "14:00 Toplantı", "Annemi ara" }, items.Select(i => i.Summary));
    }
}
