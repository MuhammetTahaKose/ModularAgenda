using ModularAgenda.Core.Abstractions;
using ModularAgenda.Core.Domain;
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
        var meeting = new TestEvent("14:00 Toplantı", Nov(24));
        var todo = new TestTodo("Ara rapor", dueDate: Nov(24));
        var note = new Note(Nov(24), "Annemi ara");
        await _events.AddAsync(meeting);
        await _todos.AddAsync(todo);
        await _notes.AddAsync(note);
        await _todos.AddAsync(new TestTodo("Başka gün", dueDate: Nov(27)));

        // Act
        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        // Assert: üç farklı sınıf aynı kutunun listesinde, ICalendarItem olarak yan yana
        Assert.Equal(new ICalendarItem[] { meeting, todo, note }, Cell(grid, Nov(24)).Items);
        Assert.Single(Cell(grid, Nov(27)).Items);
        Assert.False(Cell(grid, Nov(25)).HasItems);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetMonthGridAsync_FillsFirstAndLastCell_WhetherRepositoriesIncludeRangeEndsOrNot(bool excludesEnds)
    {
        // Arrange: ızgaranın ilk ve son kutusu; depolar aralığın uçlarını dahil etse de etmese de dolmalı
        var events = new FakeCalendarEventRepository { ExcludesRangeEnds = excludesEnds };
        var todos = new FakeTodoRepository { ExcludesRangeEnds = excludesEnds };
        var notes = new FakeNoteRepository { ExcludesRangeEnds = excludesEnds };
        var calendar = new CalendarService(events, todos, notes);
        var firstCellEvent = new TestEvent("Ekim sonu", new DateOnly(2026, 10, 26));
        var lastCellTodo = new TestTodo("Aralık başı", dueDate: new DateOnly(2026, 12, 6));
        var lastCellNote = new Note(new DateOnly(2026, 12, 6), "Aralık notu");
        await events.AddAsync(firstCellEvent);
        await todos.AddAsync(lastCellTodo);
        await notes.AddAsync(lastCellNote);

        // Act
        var grid = await calendar.GetMonthGridAsync(2026, 11, Today);

        // Assert
        Assert.Same(firstCellEvent, Assert.Single(grid[0].Items));
        Assert.Equal(new ICalendarItem[] { lastCellTodo, lastCellNote }, grid[^1].Items);
    }

    [Fact]
    public async Task GetMonthGridAsync_TodoWithoutDueDate_IsNotOnTheCalendar()
    {
        await _todos.AddAsync(new TestTodo("Tarihsiz görev"));

        var grid = await _calendar.GetMonthGridAsync(2026, 11, Today);

        Assert.DoesNotContain(grid, c => c.HasItems);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetDayAsync_ReturnsOnlyThatDaysItems_WhetherRepositoriesIncludeRangeEndsOrNot(bool excludesEnds)
    {
        // Arrange
        var events = new FakeCalendarEventRepository { ExcludesRangeEnds = excludesEnds };
        var notes = new FakeNoteRepository { ExcludesRangeEnds = excludesEnds };
        var calendar = new CalendarService(events, new FakeTodoRepository(), notes);
        var meeting = new TestEvent("14:00 Toplantı", Nov(24));
        var note = new Note(Nov(24), "Annemi ara");
        await events.AddAsync(new TestEvent("Önceki gün", Nov(23)));
        await events.AddAsync(meeting);
        await notes.AddAsync(note);
        await events.AddAsync(new TestEvent("Ertesi gün", Nov(25)));

        // Act
        var items = await calendar.GetDayAsync(Nov(24));

        // Assert
        Assert.Equal(new ICalendarItem[] { meeting, note }, items);
    }
}
