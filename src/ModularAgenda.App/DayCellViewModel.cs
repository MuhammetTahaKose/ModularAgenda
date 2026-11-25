using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModularAgenda.App;

public sealed class DayCellViewModel : INotifyPropertyChanged
{
    private bool _hasNote;
    private bool _isSelected;

    public DayCellViewModel(DateOnly date, DateOnly displayedMonth)
    {
        Date = date;
        IsInDisplayedMonth = date.Month == displayedMonth.Month && date.Year == displayedMonth.Year;
        IsToday = date == DateOnly.FromDateTime(DateTime.Today);
    }

    public DateOnly Date { get; }
    public string DayNumber => Date.Day.ToString();
    public bool IsInDisplayedMonth { get; }
    public bool IsToday { get; }

    public bool HasNote
    {
        get => _hasNote;
        set
        {
            if (_hasNote == value) return;
            _hasNote = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
