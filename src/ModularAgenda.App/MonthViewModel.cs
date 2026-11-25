using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModularAgenda.Core.Exceptions;
using ModularAgenda.Core.Services;

namespace ModularAgenda.App;

public sealed class MonthViewModel : INotifyPropertyChanged
{
    private readonly NoteService _noteService;
    private DateOnly _displayedMonth;
    private DayCellViewModel? _selectedDay;
    private string _noteText = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private int _monthRequestVersion;
    private int _dayRequestVersion;

    public MonthViewModel(NoteService noteService)
    {
        _noteService = noteService;
        _displayedMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        PreviousMonthCommand = new AsyncRelayCommand(() => ChangeMonthAsync(-1), () => !IsBusy);
        NextMonthCommand = new AsyncRelayCommand(() => ChangeMonthAsync(1), () => !IsBusy);
        SaveNoteCommand = new AsyncRelayCommand(SaveNoteAsync, () => !IsBusy && SelectedDay is not null);
        SelectDayCommand = new AsyncRelayCommand(SelectDayAsync, () => !IsBusy);
    }

    public ObservableCollection<DayCellViewModel> Days { get; } = new();
    public AsyncRelayCommand PreviousMonthCommand { get; }
    public AsyncRelayCommand NextMonthCommand { get; }
    public AsyncRelayCommand SaveNoteCommand { get; }
    public AsyncRelayCommand SelectDayCommand { get; }

    public string MonthTitle => _displayedMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
    public DateOnly? SelectedDate => SelectedDay?.Date;

    public DayCellViewModel? SelectedDay
    {
        get => _selectedDay;
        private set
        {
            if (ReferenceEquals(_selectedDay, value)) return;
            if (_selectedDay is not null) _selectedDay.IsSelected = false;
            _selectedDay = value;
            if (_selectedDay is not null) _selectedDay.IsSelected = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedDate));
            SaveNoteCommand.RaiseCanExecuteChanged();
        }
    }

    public string NoteText
    {
        get => _noteText;
        set
        {
            if (_noteText == value) return;
            _noteText = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            PreviousMonthCommand.RaiseCanExecuteChanged();
            NextMonthCommand.RaiseCanExecuteChanged();
            SaveNoteCommand.RaiseCanExecuteChanged();
            SelectDayCommand.RaiseCanExecuteChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public async Task InitializeAsync()
    {
        BuildDays();
        await LoadMonthNotesAsync(_displayedMonth, ++_monthRequestVersion);
        await SelectDayAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    private async Task ChangeMonthAsync(int monthDelta)
    {
        _displayedMonth = _displayedMonth.AddMonths(monthDelta);
        OnPropertyChanged(nameof(MonthTitle));
        BuildDays();
        await LoadMonthNotesAsync(_displayedMonth, ++_monthRequestVersion);
    }

    private void BuildDays()
    {
        Days.Clear();
        var firstDay = _displayedMonth;
        int mondayBasedOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        var gridStart = firstDay.AddDays(-mondayBasedOffset);
        for (var index = 0; index < 42; index++)
            Days.Add(new DayCellViewModel(gridStart.AddDays(index), _displayedMonth));

        SelectedDay = null;
        NoteText = string.Empty;
        StatusMessage = string.Empty;
    }

    private async Task LoadMonthNotesAsync(DateOnly month, int requestVersion)
    {
        try
        {
            IsBusy = true;
            var notes = await _noteService.GetNotesForMonthAsync(month.Year, month.Month);
            if (requestVersion != _monthRequestVersion || month != _displayedMonth) return;
            var noteDates = notes.Select(note => note.Date).ToHashSet();
            foreach (var day in Days) day.HasNote = noteDates.Contains(day.Date);
        }
        catch (Exception exception)
        {
            if (requestVersion == _monthRequestVersion) StatusMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SelectDayAsync(object? parameter)
    {
        if (parameter is DayCellViewModel day) await SelectDayAsync(day.Date);
    }

    private async Task SelectDayAsync(DateOnly date)
    {
        var day = Days.FirstOrDefault(item => item.Date == date);
        if (day is null) return;
        SelectedDay = day;
        NoteText = string.Empty;
        StatusMessage = string.Empty;
        int requestVersion = ++_dayRequestVersion;
        try
        {
            IsBusy = true;
            var note = await _noteService.GetNoteForDayAsync(date);
            if (requestVersion != _dayRequestVersion || SelectedDate != date) return;
            NoteText = note?.Text ?? string.Empty;
        }
        catch (Exception exception)
        {
            if (requestVersion == _dayRequestVersion && SelectedDate == date) StatusMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveNoteAsync()
    {
        if (SelectedDay is null) return;
        var date = SelectedDay.Date;
        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            await _noteService.SaveNoteForDayAsync(date, NoteText);
            if (SelectedDate != date) return;
            SelectedDay.HasNote = true;
            StatusMessage = "Not kaydedildi.";
        }
        catch (DomainException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (Exception exception)
        {
            StatusMessage = $"Not kaydedilemedi: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
